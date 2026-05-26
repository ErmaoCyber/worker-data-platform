export interface AuditLogRecord {
  action: string;
  employerAddress: string;
  workerAddress: string;
  transactionHash: string;
  blockHash: string | null;
  createdAt: string;
}

export interface WorkerAuditLogResponse {
  workerId: string;
  records: AuditLogRecord[];
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5258';

export async function getMyWorkerAuditLog(
  token: string,
): Promise<WorkerAuditLogResponse> {
  const response = await fetch(`${API_BASE_URL}/api/worker/audit-log/me`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error('Failed to load audit log.');
  }

  return response.json();
}
