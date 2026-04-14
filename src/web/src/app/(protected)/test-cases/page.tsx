"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { testCaseService, type TestCase } from "@/services/testcases";
import { environmentService, type Environment } from "@/services/environments";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { Plus, FileText, Trash2, Play, Search } from "lucide-react";
import { useState } from "react";

const testCaseSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  targetEnvironmentId: z.string().min(1, "Environment is required"),
  stepsJson: z.string().min(1, "Steps are required"),
  tagsJson: z.string().optional(),
});

type TestCaseForm = z.infer<typeof testCaseSchema>;

export default function TestCasesPage() {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [selectedEnvId, setSelectedEnvId] = useState<string>("");

  const { data: testCases = [], isLoading } = useQuery({
    queryKey: ["test-cases", selectedEnvId],
    queryFn: () => testCaseService.getAll(selectedEnvId || undefined),
  });

  const { data: environments = [] } = useQuery({
    queryKey: ["environments"],
    queryFn: environmentService.getAll,
  });

  const createMutation = useMutation({
    mutationFn: testCaseService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["test-cases"] });
      setOpen(false);
      toast.success("Test case created");
    },
    onError: () => toast.error("Failed to create test case"),
  });

  const deleteMutation = useMutation({
    mutationFn: testCaseService.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["test-cases"] });
      setDeleteId(null);
      toast.success("Test case deleted");
    },
    onError: () => toast.error("Failed to delete test case"),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<TestCaseForm>({
    resolver: zodResolver(testCaseSchema),
    defaultValues: {
      stepsJson: "[]",
      tagsJson: "",
    },
  });

  const onSubmit = (data: TestCaseForm) => {
    createMutation.mutate({
      name: data.name,
      description: data.description,
      targetEnvironmentId: data.targetEnvironmentId,
      stepsJson: data.stepsJson,
      tagsJson: data.tagsJson,
    });
  };

  const handleOpenCreate = () => {
    reset({
      name: "",
      description: "",
      targetEnvironmentId: "",
      stepsJson: "[]",
      tagsJson: "",
    });
    setOpen(true);
  };

  const filteredCases = testCases.filter((tc: TestCase) =>
    tc.name.toLowerCase().includes(search.toLowerCase())
  );

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <p className="text-muted-foreground text-sm">Loading test cases...</p>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Test Cases</h1>
          <p className="text-muted-foreground text-sm mt-1">
            {testCases.length} test case{testCases.length !== 1 ? "s" : ""} in library
          </p>
        </div>
        <Button onClick={handleOpenCreate} data-testid="create-test-case-button">
          <Plus className="h-4 w-4" />
          New Test Case
        </Button>
      </div>

      {/* Filters */}
      <div className="flex gap-3 flex-wrap">
        <div className="relative flex-1 max-w-xs">
          <Search className="absolute left-2.5 top-2.5 h-3.5 w-3.5 text-muted-foreground" />
          <Input
            placeholder="Search test cases..."
            className="pl-8"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            data-testid="test-case-search"
          />
        </div>
        <select
          className="h-9 rounded-md border border-input bg-transparent px-3 text-sm"
          value={selectedEnvId}
          onChange={(e) => setSelectedEnvId(e.target.value)}
          data-testid="env-filter"
        >
          <option value="">All Environments</option>
          {environments.map((env: Environment) => (
            <option key={env.id} value={env.id}>
              {env.name}
            </option>
          ))}
        </select>
      </div>

      {filteredCases.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <FileText className="h-8 w-8 text-muted-foreground mx-auto mb-3" />
            <p className="text-muted-foreground text-sm">
              {testCases.length === 0
                ? "No test cases yet. Create your first test case to get started."
                : "No test cases match your search."}
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          {filteredCases.map((tc: TestCase) => {
            let tags: string[] = [];
            try {
              tags = tc.tagsJson ? JSON.parse(tc.tagsJson) : [];
            } catch {}
            let steps: unknown[] = [];
            try {
              steps = tc.stepsJson ? JSON.parse(tc.stepsJson) : [];
            } catch {}

            return (
              <Card key={tc.id} data-testid={`tc-card-${tc.id}`}>
                <CardHeader className="flex flex-row items-start justify-between pb-2">
                  <div className="space-y-1 min-w-0 flex-1">
                    <CardTitle className="text-base">{tc.name}</CardTitle>
                    <div className="flex items-center gap-2 flex-wrap">
                      <Badge variant="outline">{tc.targetEnvironmentName}</Badge>
                      <Badge variant="secondary">{steps.length} step{steps.length !== 1 ? "s" : ""}</Badge>
                      {tags.map((tag: string) => (
                        <Badge key={tag} variant="secondary">{tag}</Badge>
                      ))}
                    </div>
                  </div>
                  <div className="flex items-center gap-1 ml-3 shrink-0">
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-muted-foreground hover:text-green-600"
                      data-testid={`run-tc-${tc.id}`}
                      title="Run test case"
                    >
                      <Play className="h-3.5 w-3.5" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-muted-foreground hover:text-destructive"
                      onClick={() => setDeleteId(tc.id)}
                      data-testid={`delete-tc-${tc.id}`}
                    >
                      <Trash2 className="h-3.5 w-3.5" />
                    </Button>
                  </div>
                </CardHeader>
                {tc.description && (
                  <CardContent className="pt-0">
                    <p className="text-sm text-muted-foreground line-clamp-2">
                      {tc.description}
                    </p>
                  </CardContent>
                )}
              </Card>
            );
          })}
        </div>
      )}

      {/* Create Dialog */}
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>New Test Case</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="tc-name">Name</Label>
              <Input
                id="tc-name"
                placeholder="Login flow"
                data-testid="tc-name-input"
                {...register("name")}
              />
              {errors.name && (
                <p className="text-xs text-destructive">{errors.name.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="tc-desc">Description</Label>
              <Input
                id="tc-desc"
                placeholder="Verify user can log in successfully"
                data-testid="tc-desc-input"
                {...register("description")}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="tc-env">Environment</Label>
              <select
                id="tc-env"
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 text-sm"
                data-testid="tc-env-select"
                {...register("targetEnvironmentId")}
              >
                <option value="">Select environment</option>
                {environments.map((env: Environment) => (
                  <option key={env.id} value={env.id}>
                    {env.name}
                  </option>
                ))}
              </select>
              {errors.targetEnvironmentId && (
                <p className="text-xs text-destructive">{errors.targetEnvironmentId.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="tc-steps">Steps (JSON)</Label>
              <textarea
                id="tc-steps"
                className="flex min-h-[100px] w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm font-mono"
                placeholder='[{"action":"Goto","value":"https://..."},{"action":"Fill","selector":"#email","value":"..."}]'
                data-testid="tc-steps-input"
                {...register("stepsJson")}
              />
              {errors.stepsJson && (
                <p className="text-xs text-destructive">{errors.stepsJson.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="tc-tags">Tags (comma-separated)</Label>
              <Input
                id="tc-tags"
                placeholder="login, auth, critical"
                data-testid="tc-tags-input"
                {...register("tagsJson")}
              />
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setOpen(false)}>
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={isSubmitting}
                data-testid="tc-submit-button"
              >
                {isSubmitting ? "Creating..." : "Create"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Delete Confirm */}
      <Dialog open={!!deleteId} onOpenChange={() => setDeleteId(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Test Case</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure? This will also remove all run history for this test case.
          </p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteId(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={() => deleteId && deleteMutation.mutate(deleteId)}
              data-testid="confirm-delete-tc-button"
            >
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
