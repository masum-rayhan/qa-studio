"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { environmentService, type Environment, type CreateEnvironmentDto } from "@/services/environments";
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
import {
  useForm,
} from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { Plus, Globe, Trash2 } from "lucide-react";
import { useState } from "react";

const envSchema = z.object({
  name: z.string().min(1, "Name is required"),
  baseUrl: z.string().url("Must be a valid URL"),
  authToken: z.string().optional(),
  isActive: z.boolean(),
});

type EnvForm = z.infer<typeof envSchema>;

export default function EnvironmentsPage() {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const { data: environments = [], isLoading } = useQuery({
    queryKey: ["environments"],
    queryFn: environmentService.getAll,
  });

  const createMutation = useMutation({
    mutationFn: (dto: CreateEnvironmentDto) => environmentService.create(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["environments"] });
      setOpen(false);
      toast.success("Environment created");
    },
    onError: () => toast.error("Failed to create environment"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => environmentService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["environments"] });
      setDeleteId(null);
      toast.success("Environment deleted");
    },
    onError: () => toast.error("Failed to delete environment"),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<EnvForm>({ resolver: zodResolver(envSchema) });

  const onSubmit = (data: EnvForm) => {
    createMutation.mutate({
      name: data.name,
      baseUrl: data.baseUrl,
      authToken: data.authToken,
      isActive: data.isActive,
    });
  };

  const handleOpenCreate = () => {
    reset({ name: "", baseUrl: "", authToken: "", isActive: true });
    setOpen(true);
  };

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <p className="text-muted-foreground text-sm">Loading environments...</p>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Environments</h1>
          <p className="text-muted-foreground text-sm mt-1">
            Manage target environments for test execution
          </p>
        </div>
        <Button onClick={handleOpenCreate} data-testid="create-environment-button">
          <Plus className="h-4 w-4" />
          Add Environment
        </Button>
      </div>

      {environments.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <Globe className="h-8 w-8 text-muted-foreground mx-auto mb-3" />
            <p className="text-muted-foreground text-sm">
              No environments configured. Add one to get started.
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {environments.map((env: Environment) => (
            <Card key={env.id} data-testid={`env-card-${env.id}`}>
              <CardHeader className="flex flex-row items-start justify-between pb-2">
                <CardTitle className="text-base">{env.name}</CardTitle>
                <div className="flex items-center gap-2">
                  <Badge variant={env.isActive ? "default" : "outline"}>
                    {env.isActive ? "Active" : "Inactive"}
                  </Badge>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8 text-muted-foreground hover:text-destructive"
                    onClick={() => setDeleteId(env.id)}
                    data-testid={`delete-env-${env.id}`}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                </div>
              </CardHeader>
              <CardContent className="space-y-1">
                <p className="text-sm font-mono text-muted-foreground truncate" title={env.baseUrl}>
                  {env.baseUrl}
                </p>
                {env.authToken && (
                  <p className="text-xs text-muted-foreground truncate">
                    Token: ••••••••
                  </p>
                )}
                <p className="text-xs text-muted-foreground">
                  Created by {env.createdByName}
                </p>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Create Dialog */}
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Environment</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Name</Label>
              <Input
                id="name"
                placeholder="Production"
                data-testid="env-name-input"
                {...register("name")}
              />
              {errors.name && (
                <p className="text-xs text-destructive">{errors.name.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="baseUrl">Base URL</Label>
              <Input
                id="baseUrl"
                placeholder="https://app.example.com"
                data-testid="env-baseurl-input"
                {...register("baseUrl")}
              />
              {errors.baseUrl && (
                <p className="text-xs text-destructive">{errors.baseUrl.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="authToken">Auth Token (optional)</Label>
              <Input
                id="authToken"
                type="password"
                placeholder="Bearer token"
                data-testid="env-token-input"
                {...register("authToken")}
              />
            </div>
            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => setOpen(false)}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={isSubmitting}
                data-testid="env-submit-button"
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
            <DialogTitle>Delete Environment</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete this environment? This action cannot be
            undone.
          </p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteId(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={() => deleteId && deleteMutation.mutate(deleteId)}
              data-testid="confirm-delete-env-button"
            >
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
