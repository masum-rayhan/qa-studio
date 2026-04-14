import api from "./api";

export interface TestCase {
  id: string;
  name: string;
  description?: string;
  stepsJson: string;
  tagsJson?: string;
  targetEnvironmentId: string;
  targetEnvironmentName: string;
  createdBy: string;
  createdByName: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTestCaseDto {
  name: string;
  description?: string;
  stepsJson: string;
  tagsJson?: string;
  targetEnvironmentId: string;
}

export interface UpdateTestCaseDto {
  name: string;
  description?: string;
  stepsJson: string;
  tagsJson?: string;
  targetEnvironmentId: string;
}

export const testCaseService = {
  getAll: async (environmentId?: string): Promise<TestCase[]> => {
    const params = environmentId ? { environmentId } : {};
    const res = await api.get<{ data: TestCase[] }>("/testcases", { params });
    return res.data.data;
  },

  getById: async (id: string): Promise<TestCase> => {
    const res = await api.get<{ data: TestCase }>(`/testcases/${id}`);
    return res.data.data;
  },

  create: async (dto: CreateTestCaseDto): Promise<TestCase> => {
    const res = await api.post<{ data: TestCase }>("/testcases", dto);
    return res.data.data;
  },

  update: async (id: string, dto: UpdateTestCaseDto): Promise<TestCase> => {
    const res = await api.put<{ data: TestCase }>(`/testcases/${id}`, dto);
    return res.data.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/testcases/${id}`);
  },
};
