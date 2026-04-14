"use client";

import { useQuery } from "@tanstack/react-query";
import { testRunService } from "@/services/testruns";
import { testCaseService } from "@/services/testcases";
import { environmentService } from "@/services/environments";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { PlayCircle, FileText, Globe, CheckCircle, XCircle, Clock } from "lucide-react";

function StatCard({
  title,
  value,
  icon: Icon,
}: {
  title: string;
  value: number;
  icon: React.ElementType;
}) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">
          {title}
        </CardTitle>
        <Icon className="h-4 w-4 text-muted-foreground" />
      </CardHeader>
      <CardContent>
        <p className="text-2xl font-bold" data-testid={`stat-${title.toLowerCase().replace(/\s+/g, "-")}`}>
          {value}
        </p>
      </CardContent>
    </Card>
  );
}

export default function DashboardPage() {
  const { data: runs = [], isLoading: runsLoading } = useQuery({
    queryKey: ["test-runs"],
    queryFn: testRunService.getAll,
  });

  const { data: testCases = [], isLoading: casesLoading } = useQuery({
    queryKey: ["test-cases"],
    queryFn: () => testCaseService.getAll(),
  });

  const { data: environments = [], isLoading: envsLoading } = useQuery({
    queryKey: ["environments"],
    queryFn: environmentService.getAll,
  });

  const isLoading = runsLoading || casesLoading || envsLoading;

  const totalRuns = runs.length;
  const passedRuns = runs.filter((r) => r.status === "Passed").length;
  const failedRuns = runs.filter((r) => r.status === "Failed").length;
  const pendingRuns = runs.filter((r) => r.status === "Pending" || r.status === "Running").length;
  const recentRuns = [...runs]
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    .slice(0, 5);

  const statusBadge = (status: string) => {
    const variants: Record<string, "default" | "secondary" | "destructive" | "outline"> = {
      Passed: "default",
      Failed: "destructive",
      Pending: "outline",
      Running: "secondary",
      Stopped: "outline",
    };
    return (
      <Badge variant={variants[status] ?? "outline"} data-testid={`badge-${status.toLowerCase()}`}>
        {status}
      </Badge>
    );
  };

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <p className="text-muted-foreground text-sm">Loading dashboard...</p>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground text-sm mt-1">
          Overview of your QA testing activity
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCard title="Total Test Cases" value={testCases.length} icon={FileText} />
        <StatCard title="Environments" value={environments.length} icon={Globe} />
        <StatCard title="Total Runs" value={totalRuns} icon={PlayCircle} />
        <StatCard
          title="Pass Rate"
          value={totalRuns > 0 ? Math.round((passedRuns / totalRuns) * 100) : 0}
          icon={CheckCircle}
        />
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <CheckCircle className="h-4 w-4 text-green-500" />
              Passed
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold" data-testid="stat-passed">{passedRuns}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <XCircle className="h-4 w-4 text-destructive" />
              Failed
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold" data-testid="stat-failed">{failedRuns}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Clock className="h-4 w-4 text-muted-foreground" />
              Pending / Running
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold" data-testid="stat-pending">{pendingRuns}</p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Recent Test Runs</CardTitle>
        </CardHeader>
        <CardContent>
          {recentRuns.length === 0 ? (
            <p className="text-sm text-muted-foreground py-4 text-center">
              No test runs yet. Run a test case to see results here.
            </p>
          ) : (
            <div className="space-y-3">
              {recentRuns.map((run) => (
                <div
                  key={run.id}
                  className="flex items-center justify-between rounded-md border px-3 py-2 text-sm"
                  data-testid={`run-row-${run.id}`}
                >
                  <div className="flex flex-col gap-0.5 min-w-0">
                    <span className="font-medium truncate">{run.testCaseName}</span>
                    <span className="text-xs text-muted-foreground truncate">
                      {run.environmentName} &middot;{" "}
                      {new Date(run.createdAt).toLocaleDateString()}
                    </span>
                  </div>
                  <div className="flex items-center gap-3 ml-4 shrink-0">
                    <span className="text-xs text-muted-foreground tabular-nums">
                      {run.totalDurationMs > 0
                        ? `${(run.totalDurationMs / 1000).toFixed(1)}s`
                        : "-"}
                    </span>
                    {statusBadge(run.status)}
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
