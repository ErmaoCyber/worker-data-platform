import type { AuditLogRecord } from '@/lib/workerAuditLogApi';

interface AuditLogViewProps {
  records: AuditLogRecord[];
}

function formatAction(action: string): string {
  const actionMap: Record<string, string> = {
    PermissionRequested: 'Access Requested',
    PermissionApproved: 'Access Approved',
    PermissionRejected: 'Request Rejected',
    DataViewed: 'Data Viewed',
    PermissionRevoked: 'Access Revoked',
  };

  return actionMap[action] ?? action;
}

function getActionDescription(action: string): string {
  const descriptionMap: Record<string, string> = {
    PermissionRequested:
      'A company requested access to some of your information.',
    PermissionApproved:
      'You approved a company to access some of your information.',
    PermissionRejected:
      'You rejected a company’s request to access your information.',
    DataViewed: 'A company viewed information that you had approved.',
    PermissionRevoked:
      'You removed a company’s access to your information.',
  };

  return (
    descriptionMap[action] ??
    'This record shows an access-related action linked to your data.'
  );
}

function shortenValue(value: string, start = 8, end = 6): string {
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
          This page shows important access-related actions recorded on
          blockchain, so you can see when your data permissions changed.
        </p>
      </section>

      {records.length === 0 ? (
        <section className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          <h2 className="text-lg font-medium text-gray-900">No Records Yet</h2>
          <p className="mt-2 text-sm text-gray-600">
            No blockchain audit records were found for your account yet.
          </p>
        </section>
      ) : (
        <section className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          <div className="mb-4">
            <h2 className="text-lg font-medium text-gray-900">
              Blockchain Records
            </h2>
            <p className="mt-1 text-sm text-gray-500">
              These records help you track important actions related to your
              data access.
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
                      {formatAction(record.action)}
                    </h3>
                    <p className="mt-1 text-sm text-gray-600">
                      {getActionDescription(record.action)}
                    </p>
                  </div>

                  <p className="text-sm text-gray-500">
                    {formatDate(record.createdAt)}
                  </p>
                </div>

                <div className="mt-4 grid gap-3 sm:grid-cols-2">
                  <div>
                    <p className="text-xs font-medium uppercase tracking-wide text-gray-500">
                      Employer
                    </p>
                    <p className="mt-1 font-mono text-sm text-gray-700">
                      {shortenValue(record.employerAddress)}
                    </p>
                  </div>

                  <div>
                    <p className="text-xs font-medium uppercase tracking-wide text-gray-500">
                      Transaction
                    </p>
                    <p className="mt-1 font-mono text-sm text-gray-700">
                      {shortenValue(record.transactionHash, 10, 8)}
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
