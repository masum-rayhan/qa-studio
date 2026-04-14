import api from "./api";

export type RunStatus = "Pending" | "Running" | "Passed" | "Failed" | "Stopped";
export type ResultStatus = "Passed" | "Failed" | "Skipped";
export type StepAction =
  | "Goto"
  | "Click"
  | "Fill"
  | "Check"
  | "Uncheck"
  | "SelectOption"
  | "WaitForURL"
  | "WaitForSelector"
  | "WaitForNavigation"
  | "WaitForTimeout"
  | "AssertVisible"
  | "AssertText"
  | "AssertURL"
  | "AssertTitle"
  | "Screenshot"
  | "Press"
  | "Scroll"
  | "Hover"
  | "DragAndDrop";

export interface TestResult {
  id: string;
  stepIndex: number;
  action: StepAction;
  selector?: string;
  value?: string;
  description?: string;
  status: ResultStatus;
  durationMs: number;
  errorMessage?: string;
  screenshotPath?: string;
}

export interface TestRun {
  id: string;
  testCaseId: string;
  testCaseName: string;
  environmentId: string;
  environmentName: string;
  status: RunStatus;
  triggeredBy: string;
  triggeredByName: string;
  startedAt?: string;
  completedAt?: string;
  totalDurationMs: number;
  videoPath?: string;
  tracePath?: string;
  createdAt: string;
  results: TestResult[];
}

export interface CreateTestRunDto {
  testCaseId: string;
  environmentId: string;
}

export const testRunService = {
  getAll: async (): Promise<TestRun[]> => {
    const res = await api.get<{ data: TestRun[] }>("/runs");
    return res.data.data;
  },

  getById: async (id: string): Promise<TestRun> => {
    const res = await api.get<{ data: TestRun }>(`/runs/${id}`);
    return res.data.data;
  },

  getByTestCase: async (testCaseId: string): Promise<TestRun[]> => {
    const res = await api.get<{ data: TestRun[] }>(`/runs/testcase/${testCaseId}`);
    return res.data.data;
  },

  create: async (dto: CreateTestRunDto): Promise<TestRun> => {
    const res = await api.post<{ data: TestRun }>("/runs", dto);
    return res.data.data;
  },

  execute: async (id: string): Promise<TestRun> => {
    const res = await api.post<{ data: TestRun }>(`/runs/${id}/execute`);
    return res.data.data;
  },
};
