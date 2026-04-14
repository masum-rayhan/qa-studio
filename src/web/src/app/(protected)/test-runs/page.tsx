"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { testRunService, type TestRun } from "@/services/testruns";
import { testCaseService } from "@/services/testcases";
import { environmentService } from "@/services/environments";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { type Environment } from "@/services/environments";
import { type TestCase } from "@/services/testcases";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
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
import { Plus, PlayCircle, Clock, CheckCircle, XCircle, Loader2 } from "lucide-react";
import { useState } from "react";

const runSchema = z.object({
  testCaseId: z.string().min(1, "Test case is required"),
  environmentId: z.string().min(1, "Environment is required"),
});

type RunForm = z.infer<typeof runSchema>;

export default function TestRunsPage() {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState("");

  const { data: runs = [], isLoading: runsLoading } = useQuery({
    queryKey: ["test-runs"],
    queryFn: testRunService.getAll,
  });

  const { data: testCases = [] } = useQuery({
    queryKey: ["test-cases"],
    queryFn: () => testCaseService.getAll(),
  });

  const { data: environments = [] } = useQuery({
    queryKey: ["environments"],
    queryFn: environmentService.getAll,
  });

  const createMutation = useMutation({
    mutationFn: testRunService.create,
    onSuccess: (run) => {
      queryClient.invalidateQueries({ queryKey: ["test-runs"] });
      setOpen(false);
      toast.success("Test run created");
      executeMutation.mutate(run.id);
    },
    onError: () => toast.error("Failed to create test run"),
  });

  const executeMutation = useMutation({
    mutationFn: testRunService.execute,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["test-runs"] });
      toast.success("Test execution started");
    },
    onError: () => toast.error("Failed to start execution"),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { isSubmitting },
  } = useForm<RunForm>({ resolver: zodResolver(runSchema) });

  const onSubmit = (data: RunForm) => {
    createMutation.mutate({
      testCaseId: data.testCaseId,
      environmentId: data.environmentId,
    });
  };

  const handleOpenCreate = () => {
    reset({ testCaseId: "", environmentId: "" });
    setOpen(true);
  };

  const filteredRuns = [...runs].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  ).filter((r: TestRun) =>
    r.testCaseName.toLowerCase().includes(search.toLowerCase()) ||
    r.environmentName.toLowerCase().includes(search.toLowerCase())
  );

  const statusConfig: Record<string, { icon: React.ElementType; color: string; label: string }> = {
    Passed: { icon: CheckCircle, color: "text-green-500", label: "Passed" },
    Failed: { icon: XCircle, color: "text-destructive", label: "Failed" },
    Running: { icon: Loader2, color: "text-blue-500 animate-spin", label: "Running" },
    Pending: { icon: Clock, color: "text-yellow-500", label: "Pending" },
    Stopped: { icon: Clock, color: "text-muted-foreground", label: "Stopped" },
  };

  if (runsLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <p className="text-muted-foreground text-sm">Loading test runs...</p>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Test Runs</h1>
          <p className="text-muted-foreground text-sm mt-1">
            {runs.length} run{runs.length !== 1 ? "s" : ""} total
          </p>
        </div>
        <Button onClick={handleOpenCreate} data-testid="new-run-button">
          <Plus className="h-4 w-4" />
          New Run
        </Button>
      </div>

      <div className="max-w-xs">
        <Input
          placeholder="Search runs..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          data-testid="runs-search"
        />
      </div>

      {filteredRuns.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <PlayCircle className="h-8 w-8 text-muted-foreground mx-auto mb-3" />
            <p className="text-muted-foreground text-sm">
              {runs.length === 0
                ? "No test runs yet. Create a run to execute a test case."
                : "No runs match your search."}
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          {filteredRuns.map((run: TestRun) => {
            const cfg = statusConfig[run.status] ?? statusConfig.Pending;
            const StatusIcon = cfg.icon;
            const passed = run.results.filter((r) => r.status === "Passed").length;
            const failed = run.results.filter((r) => r.status === "Failed").length;
            const total = run.results.length;

            return (
              <Card key={run.id} data-testid={`run-card-${run.id}`}>
                <CardHeader className="flex flex-row items-center justify-between pb-2">
                  <div className="space-y-1 min-w-0 flex-1">
                    <CardTitle className="text-base">{run.testCaseName}</CardTitle>
                    <div className="flex items-center gap-2 text-xs text-muted-foreground">
                      <span>{run.environmentName}</span>
                      <span>&middot;</span>
                      <span>{run.triggeredByName}</span>
                      <span>&middot;</span>
                      <span>{new Date(run.createdAt).toLocaleString()}</span>
                    </div>
                  </div>
                  <div className="flex items-center gap-2 ml-3 shrink-0">
                    <span className={`flex items-center gap-1 text-sm font-medium ${cfg.color}`}>
                      <StatusIcon className="h-4 w-4" />
                      {cfg.label}
                    </span>
                  </div>
                </CardHeader>
                <CardContent className="pt-0">
                  <div className="flex items-center gap-4 text-xs text-muted-foreground">
                    {total > 0 ? (
                      <>
                        <span className="text-green-600 font-medium">{passed} passed</span>
                        <span className="text-destructive font-medium">{failed} failed</span>
                        <span>{total - passed - failed} skipped</span>
                      </>
                    ) : (
                      <span>No results yet</span>
                    )}
                    {run.totalDurationMs > 0 && (
                      <span className="ml-auto">
                        {(run.totalDurationMs / 1000).toFixed(1)}s
                      </span>
                    )}
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}

      {/* Create Run Dialog */}
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>New Test Run</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="run-tc">Test Case</Label>
              <select
                id="run-tc"
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 text-sm"
                data-testid="run-tc-select"
                {...register("testCaseId")}
              >
                <option value="">Select test case</option>
                {testCases.map((tc: TestCase) => (
                  <option key={tc.id} value={tc.id}>
                    {tc.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="run-env">Environment</Label>
              <select
                id="run-env"
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 text-sm"
                data-testid="run-env-select"
                {...register("environmentId")}
              >
                <option value="">Select environment</option>
                {environments.map((env: Environment) => (
                  <option key={env.id} value={env.id}>
                    {env.name}
                  </option>
                ))}
              </select>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setOpen(false)}>
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={isSubmitting}
                data-testid="run-submit-button"
              >
                {isSubmitting ? "Starting..." : "Run"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
