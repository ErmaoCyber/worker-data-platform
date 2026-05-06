import type { WorkerDashboardResponse } from "@/lib/workerDashboardApi";

type WorkerDashboardViewProps = {
  data: WorkerDashboardResponse;
};

function formatBlockchainAction(action: string) {
  switch (action) {
    case "PermissionRequested":
      return "Permission requested";
    case "PermissionApproved":
      return "Permission approved";
    case "PermissionRejected":
      return "Permission rejected";
    case "DataViewed":
      return "Data viewed";
    case "PermissionRevoked":
      return "Permission revoked";
    default:
      return action;
  }
}

function shortenAddress(address: string) {
  if (!address) return "";
  return `${address.slice(0, 6)}...${address.slice(-4)}`;
}

function shortenTxHash(txHash: string) {
  if (!txHash) return "";
  return `${txHash.slice(0, 10)}...${txHash.slice(-6)}`;
}

export default function WorkerDashboardView({
  data,
}: WorkerDashboardViewProps) {
  return (
    <main className="min-h-screen bg-slate-50 px-8 py-8">
      <div className="mx-auto max-w-6xl space-y-6">
        <header>
          <h1 className="text-2xl font-semibold text-slate-900">
            Worker Dashboard
          </h1>
        </header>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-lg font-semibold text-slate-900">
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
                {data.worker.verified ? "Verified" : "Not verified"}
              </p>
            </div>
          </div>

          {data.worker.blockchainAddress && (
            <div className="mt-4 rounded-lg bg-slate-50 p-3">
              <p className="text-sm text-slate-500">Blockchain address</p>
              <p className="mt-1 break-all font-mono text-sm text-slate-700">
                {data.worker.blockchainAddress}
              </p>
            </div>
          )}
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-lg font-semibold text-slate-900">
            Latest Requests
          </h2>

          {data.latestRequests.length === 0 ? (
            <p className="text-sm text-slate-500">No requests yet.</p>
          ) : (
            <div className="divide-y divide-slate-200">
              {data.latestRequests.map((request) => (
                <div
                  key={request.requestId}
                  className="flex items-start justify-between gap-4 py-4 first:pt-0 last:pb-0"
                >
                  <div>
                    <p className="font-medium text-slate-900">
                      {request.employerName}
                    </p>
                    <p className="mt-1 text-sm text-slate-600">
                      {request.reason}
                    </p>
                  </div>

                  <p className="shrink-0 text-sm text-slate-500">
                    {new Date(request.createdAt).toLocaleDateString()}
                  </p>
                </div>
              ))}
            </div>
          )}
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <div className="mb-4 flex items-start justify-between gap-4">
            <div>
              <h2 className="text-lg font-semibold text-slate-900">
                Blockchain Records
              </h2>
              <p className="mt-1 text-sm text-slate-500">
                Recent permission and data access events recorded on the
                blockchain.
              </p>
            </div>

            <span
              className={`rounded-full px-3 py-1 text-xs font-medium ${data.blockchainAvailable
                  ? "bg-emerald-100 text-emerald-700"
                  : "bg-red-100 text-red-700"
                }`}
            >
              {data.blockchainAvailable ? "Connected" : "Unavailable"}
            </span>
          </div>

          {data.blockchainRecords.length === 0 ? (
            <p className="text-sm text-slate-500">
              No blockchain records available yet.
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
                        {formatBlockchainAction(record.action)}
                      </p>
                      <p className="mt-1 text-sm text-slate-500">
                        {new Date(record.date).toLocaleString()}
                      </p>
                    </div>

                    <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-medium text-slate-700">
                      On-chain
                    </span>
                  </div>

                  <div className="mt-3 grid gap-2 text-sm md:grid-cols-2">
                    <div>
                      <p className="text-slate-500">Employer address</p>
                      <p className="font-mono text-slate-700">
                        {shortenAddress(record.employerAddress)}
                      </p>
                    </div>

                    <div>
                      <p className="text-slate-500">Worker address</p>
                      <p className="font-mono text-slate-700">
                        {shortenAddress(record.workerAddress)}
                      </p>
                    </div>

                    <div className="md:col-span-2">
                      <p className="text-slate-500">Transaction hash</p>
                      <p className="break-all font-mono text-slate-700">
                        {shortenTxHash(record.txHash)}
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
