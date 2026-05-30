import Link from 'next/link';
import type { WorkerDashboardResponse } from '@/lib/workerDashboardApi';

type WorkerDashboardViewProps = {
  data: WorkerDashboardResponse;
};

function formatDateTime(dateString: string) {
  if (!dateString) return '-';

  return new Date(dateString).toLocaleString('en-NZ', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function formatDate(dateString?: string | null) {
  if (!dateString) return '-';

  return new Date(dateString).toLocaleDateString('en-NZ', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

function getStatusLabel(status: number) {
  switch (status) {
    case 0:
      return 'Pending';
    case 1:
      return 'Approved';
    case 2:
      return 'Rejected';
    case 3:
      return 'Revoked';
    default:
      return 'Unknown';
  }
}

function getStatusBadgeClass(status: number) {
  switch (status) {
    case 0:
      return 'bg-orange-100 text-orange-700';
    case 1:
      return 'bg-emerald-100 text-emerald-700';
    case 2:
      return 'bg-red-100 text-red-700';
    case 3:
      return 'bg-slate-100 text-slate-700';
    default:
      return 'bg-slate-100 text-slate-600';
  }
}

function countByStatus(
  requests: WorkerDashboardResponse['latestRequests'],
  status: number,
) {
  return requests.filter((request) => request.status === status).length;
}

function shortenTxHash(txHash: string) {
  if (!txHash) return 'Not available';
  return `${txHash.slice(0, 10)}...${txHash.slice(-6)}`;
}

export default function WorkerDashboardView({
  data,
}: WorkerDashboardViewProps) {
  const pendingCount = countByStatus(data.latestRequests, 0);
  const approvedCount = countByStatus(data.latestRequests, 1);
  const revokedCount = countByStatus(data.latestRequests, 3);

  return (
    <main className="min-h-screen bg-slate-50 px-8 py-8">
      <div className="mx-auto max-w-7xl space-y-6">
        <header className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-sm font-medium text-slate-500">
              Worker dashboard
            </p>
            <h1 className="mt-1 text-2xl font-semibold text-slate-900">
              Welcome back, {data.worker.name}
            </h1>
            <p className="mt-1 text-sm text-slate-500">
              Review data requests, monitor active access, and check your audit
              history.
            </p>
          </div>

          <Link
            href="/worker/dataAccess"
            className="rounded-xl bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800"
          >
            Manage data access
          </Link>
        </header>

        <section className="grid gap-4 md:grid-cols-4">
          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <p className="text-sm text-slate-500">Pending requests</p>
            <p className="mt-2 text-3xl font-semibold text-slate-900">
              {pendingCount}
            </p>
            <p className="mt-2 text-xs text-slate-500">
              Requests waiting for your review
            </p>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <p className="text-sm text-slate-500">Approved access</p>
            <p className="mt-2 text-3xl font-semibold text-slate-900">
              {approvedCount}
            </p>
            <p className="mt-2 text-xs text-slate-500">
              Companies currently allowed to access data
            </p>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <p className="text-sm text-slate-500">Revoked access</p>
            <p className="mt-2 text-3xl font-semibold text-slate-900">
              {revokedCount}
            </p>
            <p className="mt-2 text-xs text-slate-500">
              Access you have removed
            </p>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <p className="text-sm text-slate-500">Blockchain audit</p>
            <p className="mt-2 text-lg font-semibold text-slate-900">
              {data.blockchainAvailable ? 'Connected' : 'Unavailable'}
            </p>
            <p className="mt-2 text-xs text-slate-500">
              {data.blockchainAvailable
                ? 'On-chain records are available.'
                : 'Blockchain is not running or no records are available yet.'}
            </p>
          </div>
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <div className="mb-5 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 className="text-lg font-semibold text-slate-900">
                Latest requests
              </h2>
              <p className="mt-1 text-sm text-slate-500">
                Recent requests and decisions linked to your worker profile.
              </p>
            </div>

            <Link
              href="/worker/dataAccess"
              className="text-sm font-semibold text-blue-600 hover:text-blue-700"
            >
              View all requests
            </Link>
          </div>

          {data.latestRequests.length === 0 ? (
            <div className="rounded-xl border border-dashed border-slate-300 p-6 text-sm text-slate-500">
              No data access requests yet.
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[900px] border-collapse text-left">
                <thead>
                  <tr className="border-b border-slate-200 text-sm font-semibold text-slate-900">
                    <th className="pb-4 pr-6">Company</th>
                    <th className="pb-4 pr-6">Information</th>
                    <th className="pb-4 pr-6">Purpose</th>
                    <th className="pb-4 pr-6">Requested</th>
                    <th className="pb-4 pr-6">Status</th>
                    <th className="pb-4 pr-6">Expires</th>
                    <th className="pb-4">Action</th>
                  </tr>
                </thead>

                <tbody>
                  {data.latestRequests.map((request) => (
                    <tr
                      key={`${request.requestId}-${request.requestedInformation}`}
                      className="border-b border-slate-100 text-sm text-slate-900 last:border-b-0"
                    >
                      <td className="py-4 pr-6 font-medium">
                        {request.employerName || 'Unknown company'}
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

                      <td className="py-4 pr-6">
                        {formatDate(request.expiresAt)}
                      </td>

                      <td className="py-4">
                        {request.status === 0 ? (
                          <Link
                            href="/worker/dataAccess"
                            className="text-sm font-semibold text-blue-600 hover:text-blue-700"
                          >
                            Review
                          </Link>
                        ) : (
                          <span className="text-sm text-slate-400">-</span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        <section className="grid gap-6 lg:grid-cols-2">
          <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
            <h2 className="text-lg font-semibold text-slate-900">
              Profile summary
            </h2>

            <div className="mt-5 space-y-4">
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
                <p className="text-sm text-slate-500">Verification status</p>
                <p className="mt-1 font-medium text-slate-900">
                  {data.worker.verified ? 'Verified' : 'Not verified'}
                </p>
              </div>
            </div>

            <Link
              href="/worker/profile"
              className="mt-5 inline-block text-sm font-semibold text-blue-600 hover:text-blue-700"
            >
              Update profile
            </Link>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
            <div className="flex items-start justify-between gap-4">
              <div>
                <h2 className="text-lg font-semibold text-slate-900">
                  Recent access history
                </h2>
                <p className="mt-1 text-sm text-slate-500">
                  Blockchain audit records for access-related actions.
                </p>
              </div>

              <Link
                href="/worker/auditLog"
                className="text-sm font-semibold text-blue-600 hover:text-blue-700"
              >
                View audit log
              </Link>
            </div>

            {!data.blockchainAvailable ? (
              <div className="mt-5 rounded-xl border border-dashed border-slate-300 bg-slate-50 p-5">
                <p className="text-sm font-medium text-slate-800">
                  Blockchain audit is currently unavailable.
                </p>
                <p className="mt-1 text-sm text-slate-500">
                  Your normal access request flow still works. On-chain audit
                  records will appear here when the blockchain service is
                  running.
                </p>
              </div>
            ) : data.blockchainRecords.length === 0 ? (
              <div className="mt-5 rounded-xl border border-dashed border-slate-300 p-5 text-sm text-slate-500">
                No blockchain records available yet.
              </div>
            ) : (
              <div className="mt-5 divide-y divide-slate-100">
                {data.blockchainRecords.slice(0, 3).map((record) => (
                  <div key={record.txHash} className="py-4 first:pt-0">
                    <p className="font-medium text-slate-900">
                      {record.actionLabel}
                    </p>
                    <p className="mt-1 text-sm text-slate-600">
                      {record.userMessage}
                    </p>
                    <p className="mt-1 text-xs text-slate-500">
                      Proof: {shortenTxHash(record.txHash)}
                    </p>
                  </div>
                ))}
              </div>
            )}
          </div>
        </section>
      </div>
    </main>
  );
}
