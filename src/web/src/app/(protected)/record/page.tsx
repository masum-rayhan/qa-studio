"use client";

import { useEffect, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  recordingService,
  type RecordingSession,
  type RecordingStep,
  type CreateRecordingSessionDto,
  type SaveRecordingDto,
} from "@/services/recording";
import { environmentService, type Environment } from "@/services/environments";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import {
  useForm,
} from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import {
  Circle,
  Square,
  Type,
  CheckSquare,
  MousePointer,
  ChevronDown,
  Clock,
  Trash2,
  Play,
  Save,
  Radio,
} from "lucide-react";

const sessionSchema = z.object({
  name: z.string().min(1, "Session name is required"),
  targetUrl: z.string().optional(),
  targetEnvironmentId: z.string().optional(),
});

type SessionForm = z.infer<typeof sessionSchema>;

const actionIcons: Record<string, React.ElementType> = {
  Goto: Circle,
  Click: MousePointer,
  Fill: Type,
  Check: CheckSquare,
  Uncheck: CheckSquare,
  SelectOption: ChevronDown,
  WaitForURL: Clock,
  WaitForSelector: Circle,
  WaitForNavigation: Circle,
  WaitForTimeout: Clock,
  AssertVisible: Circle,
  AssertHidden: Circle,
  AssertText: Type,
  AssertContainsText: Type,
  AssertURL: Circle,
  AssertTitle: Type,
  Screenshot: Square,
  Press: Circle,
  Scroll: Circle,
  Hover: MousePointer,
  DragAndDrop: MousePointer,
};

function StepRow({ step, index }: { step: RecordingStep; index: number }) {
  const Icon = actionIcons[step.action] ?? Circle;
  return (
    <div
      className="flex items-start gap-3 px-3 py-2 border-b border-border last:border-0"
      data-testid={`step-row-${index}`}
    >
      <Icon className="h-4 w-4 mt-0.5 shrink-0 text-muted-foreground" />
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <Badge variant="secondary" className="text-xs">{step.action}</Badge>
          {step.selector && (
            <code className="text-xs text-muted-foreground truncate max-w-[200px]">
              {step.selector}
            </code>
          )}
        </div>
        {step.value && (
          <p className="text-xs text-muted-foreground mt-0.5 truncate">
            {step.value}
          </p>
        )}
        {step.description && (
          <p className="text-xs text-muted-foreground mt-0.5 italic">
            {step.description}
          </p>
        )}
      </div>
      <span className="text-xs text-muted-foreground tabular-nums shrink-0">
        #{index + 1}
      </span>
    </div>
  );
}

export default function RecordPage() {
  const [activeSession, setActiveSession] = useState<RecordingSession | null>(null);
  const [steps, setSteps] = useState<RecordingStep[]>([]);
  const [sessionName, setSessionName] = useState("");
  const [openSave, setOpenSave] = useState(false);
  const eventSourceRef = useRef<EventSource | null>(null);

  const { data: environments = [] } = useQuery({
    queryKey: ["environments"],
    queryFn: environmentService.getAll,
  });

  const { data: pastSessions = [] } = useQuery({
    queryKey: ["recording-sessions"],
    queryFn: recordingService.getMySessions,
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<SessionForm>({ resolver: zodResolver(sessionSchema) });

  const startSession = async (data: SessionForm) => {
    const dto: CreateRecordingSessionDto = {
      name: data.name,
      targetUrl: data.targetUrl,
      targetEnvironmentId: data.targetEnvironmentId || undefined,
    };
    const session = await recordingService.startSession(dto);
    setActiveSession(session);
    setSessionName(session.name);
    setSteps([]);
    toast.success("Recording session started");
    reset();
  };

  const openEventSource = (sessionId: string) => {
    const es = new EventSource(
      `${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000/api"}/recording/${sessionId}/events`,
      { withCredentials: true }
    );

    es.addEventListener("message", (e) => {
      try {
        const payload = JSON.parse(e.data);
        if (payload.steps) {
          setSteps(payload.steps);
        }
      } catch {}
    });

    es.addEventListener("closed", (e) => {
      try {
        const payload = JSON.parse((e as MessageEvent).data);
        if (payload.status && payload.status !== "Active") {
          setActiveSession((s) => s ? { ...s, status: payload.status } : s);
        }
      } catch {}
      es.close();
    });

    es.onerror = () => {
      es.close();
    };

    eventSourceRef.current = es;
  };

  useEffect(() => {
    if (activeSession && activeSession.status === "Active") {
      // Load initial steps
      recordingService.getSession(activeSession.id).then((s) => {
        setSteps(JSON.parse(s.stepsJson || "[]"));
      });
      openEventSource(activeSession.id);
    }
    return () => {
      eventSourceRef.current?.close();
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeSession?.id]);

  const handleStop = async () => {
    if (!activeSession) return;
    await recordingService.completeSession(activeSession.id);
    setActiveSession((s) => s ? { ...s, status: "Completed" } : s);
    eventSourceRef.current?.close();
    toast.success("Recording stopped");
  };

  const handleDelete = async () => {
    if (!activeSession) return;
    await recordingService.deleteSession(activeSession.id);
    eventSourceRef.current?.close();
    setActiveSession(null);
    setSteps([]);
    setSessionName("");
    toast.success("Session deleted");
  };

  const {
    register: registerSave,
    handleSubmit: handleSaveSubmit,
    formState: { errors: saveErrors },
  } = useForm<{ targetEnvironmentId: string; tagsJson: string }>({
    defaultValues: {
      targetEnvironmentId: activeSession?.targetEnvironmentId ?? "",
      tagsJson: "",
    },
  });

  const handleSave = async (data: { targetEnvironmentId: string; tagsJson: string }) => {
    if (!activeSession) return;
    const dto: SaveRecordingDto = {
      targetEnvironmentId: data.targetEnvironmentId,
      tagsJson: data.tagsJson || undefined,
    };
    await recordingService.saveAsTestCase(activeSession.id, dto);
    setActiveSession(null);
    setSteps([]);
    setSessionName("");
    setOpenSave(false);
    toast.success("Test case saved!");
  };

  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Record</h1>
        <p className="text-muted-foreground text-sm mt-1">
          Record test steps directly from the browser
        </p>
      </div>

      {!activeSession ? (
        <div className="grid gap-6 lg:grid-cols-2">
          {/* Start New Session */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base flex items-center gap-2">
                <Radio className="h-4 w-4 text-red-500" />
                New Recording Session
              </CardTitle>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleSubmit(startSession)} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="sess-name">Session Name</Label>
                  <Input
                    id="sess-name"
                    placeholder="Login flow recording"
                    data-testid="session-name-input"
                    {...register("name")}
                  />
                  {errors.name && (
                    <p className="text-xs text-destructive">{errors.name.message}</p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="sess-url">Target URL (optional)</Label>
                  <Input
                    id="sess-url"
                    placeholder="https://app.bentostudio.com"
                    data-testid="session-url-input"
                    {...register("targetUrl")}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="sess-env">Target Environment (optional)</Label>
                  <select
                    id="sess-env"
                    className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 text-sm"
                    data-testid="session-env-select"
                    {...register("targetEnvironmentId")}
                  >
                    <option value="">Select environment</option>
                    {environments.map((env: Environment) => (
                      <option key={env.id} value={env.id}>
                        {env.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="bg-muted/50 rounded-md p-3 text-xs text-muted-foreground">
                  <p className="font-medium text-foreground mb-1">How to record:</p>
                  <ol className="list-decimal list-inside space-y-1">
                    <li>Start a session here</li>
                    <li>Open the QA Studio browser extension</li>
                    <li>Paste the session ID shown below</li>
                    <li>Navigate to the target page and interact</li>
                    <li>Steps appear in real-time below</li>
                  </ol>
                </div>
                <Button
                  type="submit"
                  className="w-full"
                  disabled={isSubmitting}
                  data-testid="start-session-button"
                >
                  <Play className="h-4 w-4" />
                  {isSubmitting ? "Starting..." : "Start Recording"}
                </Button>
              </form>
            </CardContent>
          </Card>

          {/* Past Sessions */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Past Sessions</CardTitle>
            </CardHeader>
            <CardContent>
              {pastSessions.length === 0 ? (
                <p className="text-sm text-muted-foreground text-center py-6">
                  No past sessions yet.
                </p>
              ) : (
                <div className="space-y-2">
                  {pastSessions.map((s) => (
                    <div
                      key={s.id}
                      className="flex items-center justify-between rounded-md border px-3 py-2 text-sm"
                      data-testid={`past-session-${s.id}`}
                    >
                      <div className="min-w-0">
                        <p className="font-medium truncate">{s.name}</p>
                        <p className="text-xs text-muted-foreground">
                          {new Date(s.createdAt).toLocaleDateString()} &middot;{" "}
                          {JSON.parse(s.stepsJson || "[]").length} steps
                        </p>
                      </div>
                      <Badge
                        variant={
                          s.status === "Completed"
                            ? "default"
                            : s.status === "Cancelled"
                            ? "destructive"
                            : "secondary"
                        }
                      >
                        {s.status}
                      </Badge>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      ) : (
        <div className="grid gap-6 lg:grid-cols-3">
          {/* Active Session Info */}
          <Card className="lg:col-span-1">
            <CardHeader>
              <CardTitle className="text-base flex items-center gap-2">
                {activeSession.status === "Active" && (
                  <span className="relative flex h-2.5 w-2.5">
                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
                    <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-red-500"></span>
                  </span>
                )}
                {sessionName || "Recording"}
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="bg-muted/50 rounded-md p-3 text-center">
                <p className="text-xs text-muted-foreground mb-1">Session ID</p>
                <code
                  className="text-sm font-mono select-all"
                  data-testid="active-session-id"
                >
                  {activeSession.id}
                </code>
              </div>
              <div className="space-y-1">
                <p className="text-xs text-muted-foreground">Extension Setup</p>
                <p className="text-xs text-muted-foreground">
                  Open the QA Studio extension in Chrome, paste the session ID above,
                  then start navigating.
                </p>
              </div>
              <div className="space-y-1">
                <p className="text-xs text-muted-foreground">Status</p>
                <Badge
                  variant={
                    activeSession.status === "Active"
                      ? "secondary"
                      : activeSession.status === "Completed"
                      ? "default"
                      : "destructive"
                  }
                >
                  {activeSession.status}
                </Badge>
              </div>
              {activeSession.status === "Active" && (
                <div className="flex flex-col gap-2">
                  <Button
                    variant="destructive"
                    size="sm"
                    onClick={handleStop}
                    data-testid="stop-recording-button"
                  >
                    Stop Recording
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={handleDelete}
                    className="text-destructive hover:text-destructive"
                    data-testid="delete-session-button"
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                    Discard
                  </Button>
                </div>
              )}
              {activeSession.status === "Completed" && (
                <Button
                  size="sm"
                  onClick={() => setOpenSave(true)}
                  data-testid="save-as-testcase-button"
                >
                  <Save className="h-4 w-4" />
                  Save as Test Case
                </Button>
              )}
            </CardContent>
          </Card>

          {/* Step List */}
          <Card className="lg:col-span-2">
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="text-base">
                Steps ({steps.length})
              </CardTitle>
              {activeSession.status === "Active" && (
                <Badge variant="secondary" className="animate-pulse">
                  Live
                </Badge>
              )}
            </CardHeader>
            <CardContent className="p-0">
              {steps.length === 0 ? (
                <div className="py-12 text-center">
                  <p className="text-sm text-muted-foreground">
                    No steps recorded yet. Interact with the page to see steps appear.
                  </p>
                </div>
              ) : (
                <div className="max-h-[500px] overflow-y-auto">
                  {steps.map((step, i) => (
                    <StepRow key={i} step={step} index={i} />
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      )}

      {/* Save Dialog */}
      <Dialog open={openSave} onOpenChange={setOpenSave}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Save as Test Case</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSaveSubmit(handleSave)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="save-env">Target Environment</Label>
              <select
                id="save-env"
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 text-sm"
                data-testid="save-env-select"
                {...registerSave("targetEnvironmentId")}
              >
                <option value="">Select environment</option>
                {environments.map((env: Environment) => (
                  <option key={env.id} value={env.id}>
                    {env.name}
                  </option>
                ))}
              </select>
              {saveErrors.targetEnvironmentId && (
                <p className="text-xs text-destructive">Environment is required</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="save-tags">Tags (comma-separated)</Label>
              <Input
                id="save-tags"
                placeholder="login, critical"
                data-testid="save-tags-input"
                {...registerSave("tagsJson")}
              />
            </div>
            <div className="bg-muted/50 rounded-md p-3">
              <p className="text-xs text-muted-foreground">
                {steps.length} step{steps.length !== 1 ? "s" : ""} will be saved
              </p>
            </div>
            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => setOpenSave(false)}
              >
                Cancel
              </Button>
              <Button type="submit" data-testid="confirm-save-button">
                Save Test Case
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
