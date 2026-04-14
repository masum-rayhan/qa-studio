import api from "./api";

export interface RecordingSession {
  id: string;
  name: string;
  targetUrl?: string;
  stepsJson: string;
  status: "Active" | "Completed" | "Cancelled";
  createdBy: string;
  createdByName: string;
  targetEnvironmentId?: string;
  targetEnvironmentName?: string;
  createdAt: string;
}

export interface RecordingStep {
  action: string;
  selector?: string;
  value?: string;
  description?: string;
}

export interface CreateRecordingSessionDto {
  name: string;
  targetUrl?: string;
  targetEnvironmentId?: string;
}

export interface AddRecordingStepDto {
  action: string;
  selector?: string;
  value?: string;
  description?: string;
}

export interface SaveRecordingDto {
  targetEnvironmentId: string;
  tagsJson?: string;
}

export const recordingService = {
  startSession: async (dto: CreateRecordingSessionDto): Promise<RecordingSession> => {
    const res = await api.post<{ data: RecordingSession }>("/recording/start", dto);
    return res.data.data;
  },

  getSession: async (id: string): Promise<RecordingSession> => {
    const res = await api.get<{ data: RecordingSession }>(`/recording/${id}`);
    return res.data.data;
  },

  getMySessions: async (): Promise<RecordingSession[]> => {
    const res = await api.get<{ data: RecordingSession[] }>("/recording");
    return res.data.data;
  },

  addStep: async (id: string, dto: AddRecordingStepDto): Promise<RecordingSession> => {
    const res = await api.post<{ data: RecordingSession }>(`/recording/${id}/steps`, dto);
    return res.data.data;
  },

  deleteSession: async (id: string): Promise<void> => {
    await api.delete(`/recording/${id}`);
  },

  completeSession: async (id: string): Promise<RecordingSession> => {
    const res = await api.post<{ data: RecordingSession }>(`/recording/${id}/complete`);
    return res.data.data;
  },

  saveAsTestCase: async (id: string, dto: SaveRecordingDto) => {
    const res = await api.post<{ data: unknown }>(`/recording/${id}/save`, dto);
    return res.data.data;
  },

  getEventSource: (sessionId: string): EventSource => {
    return new EventSource(
      `${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000/api"}/recording/${sessionId}/events`,
      { withCredentials: true }
    );
  },
};
