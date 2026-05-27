import type { AuditLogRecord } from '@/lib/workerAuditLogApi';

interface AuditLogViewProps {
  records: AuditLogRecord[];
}

function shortenValue(value: string, start = 8, end = 6): string {
  if (!value) {
    return 'Not available';
  }

  if (value.length <= start + end + 3) {
    return value;
  }

  return `${value.slice(0, start)}...${value.slice(-end)}`;
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleString();
}

export default function AuditLogView({ records }: AuditLogViewProps) {
  return (
    <main className="space-y-6 p-6">
      <section>
        <h1 className="text-2xl font-semibold text-gray-900">Audit Log</h1>
        <p className="mt-1 text-sm text-gray-600">
          This page shows important access-related actions, so you can check
          which company interacted with your data and when it happened.
        </p>
      </section>

      {records.length === 0 ? (
        <section className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          <h2 className="text-lg font-medium text-gray-900">No Records Yet</h2>
          <p className="mt-2 text-sm text-gray-600">
            No audit records were found for your account yet.
          </p>
        </section>
      ) : (
        <section className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          <div className="mb-5">
            <h2 className="text-lg font-medium text-gray-900">
              Access History
            </h2>
            <p className="mt-1 text-sm text-gray-500">
              These records show company access-related actions. The transaction
              hash is kept as blockchain proof.
            </p>
          </div>

          <div className="space-y-4">
            {records.map((record) => (
              <article
                key={record.transactionHash}
                className="rounded-lg border border-gray-100 bg-gray-50 p-4"
              >
                <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <h3 className="text-base font-semibold text-gray-900">
                      {record.actionLabel}
                    </h3>
                    <p className="mt-1 text-sm text-gray-700">
                      {record.userMessage}
                    </p>
                  </div>

                  <p className="text-sm text-gray-500">
                    {formatDate(record.createdAt)}
                  </p>
                </div>

                <div className="mt-4 grid gap-3 sm:grid-cols-3">
                  <div>
                    <p className="text-xs font-medium uppercase tracking-wide text-gray-500">
                      Company
                    </p>
                    <p className="mt-1 text-sm font-medium text-gray-800">
                      {record.employerName}
                    </p>
                  </div>

                  <div>
                    <p className="text-xs font-medium uppercase tracking-wide text-gray-500">
                      Blockchain proof
                    </p>
                    <p className="mt-1 font-mono text-sm text-gray-700">
                      {shortenValue(record.transactionHash, 10, 8)}
                    </p>
                  </div>

                  <div>
                    <p className="text-xs font-medium uppercase tracking-wide text-gray-500">
                      Technical address
                    </p>
                    <p className="mt-1 font-mono text-sm text-gray-500">
                      {shortenValue(record.employerAddress)}
                    </p>
                  </div>
                </div>
              </article>
            ))}
          </div>
        </section>
      )}
    </main>
  );
}
