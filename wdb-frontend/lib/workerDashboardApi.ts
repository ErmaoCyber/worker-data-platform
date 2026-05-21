const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5258";

const BASE_URL = `${API_BASE_URL}/api/worker/dashboard`;

export type WorkerBasicInfo = {
  id: string;
  name: string;
  email: string;
  verified: boolean;
  blockchainAddress?: string | null;
};

export type WorkerDashboardRequest = {
  requestId: string;
  employerId: string;
  employerName: string;
  requestedInformation: string;
  checkPurpose: string;
  createdAt: string;
  status: number;
  expiresAt?: string | null;
};

export type BlockchainRecord = {
  employerAddress: string;
  workerAddress: string;
  action: string;
  txHash: string;
  date: string;
};

export type WorkerDashboardResponse = {
  worker: WorkerBasicInfo;
  latestRequests: WorkerDashboardRequest[];
  blockchainRecords: BlockchainRecord[];
  blockchainAvailable: boolean;
};

export async function getWorkerDashboard(
  workerId: string
): Promise<WorkerDashboardResponse> {
  const response = await fetch(`${BASE_URL}/${workerId}`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error("Failed to fetch worker dashboard");
  }

  return response.json();
}
