export interface WorkerDashboardResponse {
  worker: WorkerBasicInfo;
  latestRequests: WorkerDashboardRequest[];
  blockchainRecords: BlockchainRecord[];
  blockchainAvailable: boolean;
}

export interface WorkerBasicInfo {
  id: string;
  name: string;
  email: string;
  verified: boolean;
  blockchainAddress?: string | null;
}

export interface WorkerDashboardRequest {
  requestId: string;
  employerId: string;
  employerName: string;
  requestedInformation: string;
  checkPurpose: string;
  createdAt: string;
  status: number;
  expiresAt?: string | null;
}

export interface BlockchainRecord {
  action: string;
  actionLabel: string;
  userMessage: string;
  employerName: string;
  employerAddress: string;
  workerAddress: string;
  txHash: string;
  date: string;
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5258';

export async function getWorkerDashboard(
  workerId: string,
): Promise<WorkerDashboardResponse> {
  const response = await fetch(
    `${API_BASE_URL}/api/worker/dashboard/${workerId}`,
    {
      method: 'GET',
    },
  );

  if (!response.ok) {
    throw new Error('Failed to load worker dashboard.');
  }

  return response.json();
}
