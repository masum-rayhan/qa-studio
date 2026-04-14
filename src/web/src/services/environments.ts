import api from "./api";

export interface Environment {
  id: string;
  name: string;
  baseUrl: string;
  authToken?: string;
  isActive: boolean;
  createdBy: string;
  createdByName: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEnvironmentDto {
  name: string;
  baseUrl: string;
  authToken?: string;
  isActive?: boolean;
}

export interface UpdateEnvironmentDto {
  name: string;
  baseUrl: string;
  authToken?: string;
  isActive?: boolean;
}

export const environmentService = {
  getAll: async (): Promise<Environment[]> => {
    const res = await api.get<{ data: Environment[] }>("/environments");
    return res.data.data;
  },

  getById: async (id: string): Promise<Environment> => {
    const res = await api.get<{ data: Environment }>(`/environments/${id}`);
    return res.data.data;
  },

  create: async (dto: CreateEnvironmentDto): Promise<Environment> => {
    const res = await api.post<{ data: Environment }>("/environments", dto);
    return res.data.data;
  },

  update: async (id: string, dto: UpdateEnvironmentDto): Promise<Environment> => {
    const res = await api.put<{ data: Environment }>(`/environments/${id}`, dto);
    return res.data.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/environments/${id}`);
  },
};
