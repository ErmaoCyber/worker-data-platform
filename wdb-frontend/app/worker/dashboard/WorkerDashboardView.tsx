import Link from 'next/link';
import type { WorkerDashboardResponse } from '@/lib/workerDashboardApi';

type WorkerDashboardViewProps = {
  data: WorkerDashboardResponse;
};

function shortenAddress(address: string) {
  if (!address) return 'Not available';
  return `${address.slice(0, 6)}...${address.slice(-4)}`;
}

function shortenTxHash(txHash: string) {
  if (!txHash) return 'Not available';
  return `${txHash.slice(0, 10)}...${txHash.slice(-6)}`;
}

function formatDateTime(dateString: string) {
  if (!dateString) return '-';

  return new Date(dateString).toLocaleString('en-NZ', {
    year: 'numeric',
    month: 'numeric',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function formatDate(dateString?: string | null) {
  if (!dateString) return '-';

  return new Date(dateString).toLocaleDateString('en-NZ', {
    year: 'numeric',
    month: 'numeric',
    day: 'numeric',
  });
}

function getStatusLabel(status: number) {
  switch (status) {
    case 1:
      return 'Approved';
    case 2:
      return 'Rejected';
    default:
      return 'Pending';
  }
}

function getStatusBadgeClass(status: number) {
  switch (status) {
    case 1:
      return 'bg-emerald-100 text-emerald-700';
    case 2:
      return 'bg-red-100 text-red-700';
    default:
      return 'bg-orange-100 text-orange-700';
  }
}

function UserIcon() {
  return (
    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-slate-200 text-slate-700">
      <svg
        xmlns="http://www.w3.org/2000/svg"
        className="h-5 w-5"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        strokeWidth={1.8}
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          d="M15.75 7.5a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.5 20.25a7.5 7.5 0 0115 0"
        />
      </svg>
    </div>
  );
}

export default function WorkerDashboardView({
  data,
}: WorkerDashboardViewProps) {
  return (
    <main className="min-h-screen bg-slate-50 px-12 py-10">
      <div className="mx-auto max-w-7xl space-y-6">
        <header className="flex items-center justify-between">
          <h1 className="text-2xl font-semibold text-slate-900">
            Worker Dashboard
          </h1>

          <UserIcon />
        </header>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="mb-6 text-lg font-semibold text-slate-900">
            Personal Information
          </h2>

          <div className="grid gap-4 md:grid-cols-3">
            <div>
              <p className="text-sm text-slate-500">Name</p>
              <p className="mt-1 font-medium text-slate-900">
                {data.worker.name}
              </p>
            </div>

            <div>
              <p className="text-sm text-slate-500">Email</p>
              <p className="mt-1 font-medium text-slate-900">
                {data.worker.email}
              </p>
            </div>

            <div>
              <p className="text-sm text-slate-500">Status</p>
              <p className="mt-1 font-medium text-slate-900">
                {data.worker.verified ? 'Verified' : 'Not verified'}
              </p>
            </div>
          </div>
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <div className="mb-6 flex items-center justify-between">
            <h2 className="text-lg font-semibold text-slate-900">
              Latest Requests
            </h2>

            <Link
              href="/worker/dataAccess"
              className="text-sm font-semibold text-blue-600 hover:text-blue-700"
            >
              View all
            </Link>
          </div>

          {data.latestRequests.length === 0 ? (
            <p className="text-sm text-slate-500">No requests yet.</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[900px] border-collapse text-left">
                <thead>
                  <tr className="border-b border-slate-200 text-sm font-semibold text-slate-900">
                    <th className="pb-4 pr-6">Company</th>
                    <th className="pb-4 pr-6">Requested Information</th>
                    <th className="pb-4 pr-6">Check Purpose</th>
                    <th className="pb-4 pr-6">Requested At</th>
                    <th className="pb-4 pr-6">Status</th>
                    <th className="pb-4">Expires At</th>
                  </tr>
                </thead>

                <tbody>
                  {data.latestRequests.map((request) => (
                    <tr
                      key={`${request.requestId}-${request.requestedInformation}`}
                      className="border-b border-slate-200 text-sm text-slate-900"
                    >
                      <td className="py-4 pr-6 font-medium">
                        {request.employerName}
                      </td>

                      <td className="py-4 pr-6">
                        {request.requestedInformation || '-'}
                      </td>

                      <td className="py-4 pr-6">
                        {request.checkPurpose || '-'}
                      </td>

                      <td className="py-4 pr-6">
                        {formatDateTime(request.createdAt)}
                      </td>

                      <td className="py-4 pr-6">
                        <span
                          className={`rounded-full px-3 py-1 text-xs font-medium ${getStatusBadgeClass(
                            request.status,
                          )}`}
                        >
                          {getStatusLabel(request.status)}
                        </span>
                      </td>

                      <td className="py-4">
                        {formatDate(request.expiresAt)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <div className="mb-4 flex items-start justify-between gap-4">
            <div>
              <h2 className="text-lg font-semibold text-slate-900">
                Recent Access History
              </h2>

              <p className="mt-1 text-sm text-slate-500">
                Recent company access-related actions recorded on blockchain.
              </p>
            </div>

            <div className="flex items-center gap-3">
              <Link
                href="/worker/auditLog"
                className="text-sm font-semibold text-blue-600 hover:text-blue-700"
              >
                View all
              </Link>

              <span
                className={`rounded-full px-3 py-1 text-xs font-medium ${data.blockchainAvailable
                    ? 'bg-emerald-100 text-emerald-700'
                    : 'bg-red-100 text-red-700'
                  }`}
              >
                {data.blockchainAvailable ? 'Connected' : 'Unavailable'}
              </span>
            </div>
          </div>

          {data.blockchainRecords.length === 0 ? (
            <p className="text-sm text-slate-500">
              No access history records available yet.
            </p>
          ) : (
            <div className="divide-y divide-slate-200">
              {data.blockchainRecords.map((record) => (
                <div
                  key={record.txHash}
                  className="py-4 first:pt-0 last:pb-0"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="font-medium text-slate-900">
                        {record.actionLabel}
                      </p>

                      <p className="mt-1 text-sm text-slate-600">
                        {record.userMessage}
                      </p>

                      <p className="mt-1 text-sm text-slate-500">
                        {formatDateTime(record.date)}
                      </p>
                    </div>

                    <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-medium text-slate-700">
                      On-chain
                    </span>
                  </div>

                  <div className="mt-3 grid gap-2 text-sm md:grid-cols-3">
                    <div>
                      <p className="text-slate-500">Company</p>
                      <p className="font-medium text-slate-800">
                        {record.employerName}
                      </p>
                    </div>

                    <div>
                      <p className="text-slate-500">Blockchain proof</p>
                      <p className="font-mono text-slate-700">
                        {shortenTxHash(record.txHash)}
                      </p>
                    </div>

                    <div>
                      <p className="text-slate-500">Technical address</p>
                      <p className="font-mono text-slate-500">
                        {shortenAddress(record.employerAddress)}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </main>
  );
}
