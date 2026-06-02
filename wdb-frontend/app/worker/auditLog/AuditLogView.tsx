'use client';

import { useMemo, useState } from 'react';
import type { AuditLogRecord } from '@/lib/workerAuditLogApi';

interface AuditLogViewProps {
  records: AuditLogRecord[];
}

const PAGE_SIZE = 6;

function shortenValue(value?: string | null, start = 10, end = 8): string {
  if (!value) return 'Not available';

  if (value.length <= start + end + 3) {
    return value;
  }

  return `${value.slice(0, start)}...${value.slice(-end)}`;
}

function formatDateTime(dateString: string): string {
  if (!dateString) return '-';

  return new Date(dateString).toLocaleString('en-NZ', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function getActionTone(action: string) {
  const normalised = action?.trim().toLowerCase() ?? '';

  // Check RequestReviewed before "view".
  // "RequestReviewed" contains "view", so otherwise it is wrongly shown as Viewed.
  if (normalised === 'requestreviewed') {
    return {
      label: 'Reviewed',
      badgeClass: 'bg-indigo-50 text-indigo-700',
      dotClass: 'bg-indigo-500',
    };
  }

  if (normalised.includes('approve')) {
    return {
      label: 'Approved',
      badgeClass: 'bg-emerald-50 text-emerald-700',
      dotClass: 'bg-emerald-500',
    };
  }

  if (normalised.includes('reject')) {
    return {
      label: 'Rejected',
      badgeClass: 'bg-red-50 text-red-700',
      dotClass: 'bg-red-500',
    };
  }

  if (normalised.includes('revoke')) {
    return {
      label: 'Revoked',
      badgeClass: 'bg-slate-100 text-slate-700',
      dotClass: 'bg-slate-500',
    };
  }

  if (normalised.includes('view')) {
    return {
      label: 'Viewed',
      badgeClass: 'bg-blue-50 text-blue-700',
      dotClass: 'bg-blue-500',
    };
  }

  if (normalised.includes('request')) {
    return {
      label: 'Requested',
      badgeClass: 'bg-orange-50 text-orange-700',
      dotClass: 'bg-orange-500',
    };
  }

  return {
    label: 'Recorded',
    badgeClass: 'bg-slate-100 text-slate-700',
    dotClass: 'bg-slate-400',
  };
}

function getDisplayTitle(record: AuditLogRecord) {
  return record.actionLabel || getActionTone(record.action).label;
}

function getDisplayMessage(record: AuditLogRecord) {
  if (record.userMessage) return record.userMessage;

  return `${record.employerName || 'A company'} performed an access-related action.`;
}

function actionItemHeading(action: string) {
  switch (action) {
    case 'PermissionRequested':
      return 'Requested information';
    case 'PermissionApproved':
      return 'Approved information';
    case 'PermissionRejected':
      return 'Rejected information';
    case 'DataViewed':
      return 'Viewed information';
    case 'PermissionRevoked':
      return 'Revoked information';
    case 'RequestReviewed':
      return 'Review summary';
    default:
      return 'Information involved';
  }
}

function getRecordTypeLabel(record: AuditLogRecord) {
  if (record.action === 'RequestReviewed') {
    return 'Request review';
  }

  return record.categoryLabel || 'Information';
}

function formatReviewSummaryItem(item: string) {
  const [section, detail] = item.split('|').map((part) => part.trim());

  if (!detail) {
    return {
      heading: 'Review detail',
      body: item,
    };
  }

  switch (section) {
    case 'APPROVED':
      return {
        heading: 'Approved items',
        body: detail,
      };
    case 'REJECTED':
      return {
        heading: 'Rejected items',
        body: detail,
      };
    case 'CUSTOM_REQUEST':
      return {
        heading: 'Custom request',
        body: detail,
      };
    default:
      return {
        heading: section || 'Review detail',
        body: detail,
      };
  }
}

export default function AuditLogView({ records }: AuditLogViewProps) {
  const [selectedAction, setSelectedAction] = useState('All');
  const [companySearch, setCompanySearch] = useState('');
  const [expandedKey, setExpandedKey] = useState<string | null>(null);
  const [page, setPage] = useState(1);

  const actionOptions = useMemo(() => {
    const actions = records.map((record) => getActionTone(record.action).label);
    return ['All', ...Array.from(new Set(actions))];
  }, [records]);

  const filteredRecords = useMemo(() => {
    return records.filter((record) => {
      const actionLabel = getActionTone(record.action).label;

      const matchesAction =
        selectedAction === 'All' || actionLabel === selectedAction;

      const matchesCompany = (record.employerName || '')
        .toLowerCase()
        .includes(companySearch.toLowerCase());

      return matchesAction && matchesCompany;
    });
  }, [records, selectedAction, companySearch]);

  const totalPages = Math.max(1, Math.ceil(filteredRecords.length / PAGE_SIZE));

  const pagedRecords = filteredRecords.slice(
    (page - 1) * PAGE_SIZE,
    page * PAGE_SIZE,
  );

  const latestRecordDate =
    records.length > 0 ? formatDateTime(records[0].createdAt) : '-';

  function handleActionChange(value: string) {
    setSelectedAction(value);
    setPage(1);
    setExpandedKey(null);
  }

  function handleCompanySearchChange(value: string) {
    setCompanySearch(value);
    setPage(1);
    setExpandedKey(null);
  }

  function handlePreviousPage() {
    setPage((current) => Math.max(1, current - 1));
    setExpandedKey(null);
  }

  function handleNextPage() {
    setPage((current) => Math.min(totalPages, current + 1));
    setExpandedKey(null);
  }

  return (
    <main className="min-h-screen bg-slate-50 px-8 py-8">
      <div className="mx-auto max-w-6xl space-y-6">
        <header className="border-b border-slate-200 pb-5">
          <p className="text-sm font-medium text-slate-500">Worker portal</p>

          <h1 className="mt-1 text-2xl font-semibold text-slate-900">
            Audit Log
          </h1>

          <p className="mt-2 max-w-3xl text-sm text-slate-500">
            Review which companies requested, viewed, or lost access to your
            data. Blockchain proof is kept in the background for verification.
          </p>

          <p className="mt-3 text-sm text-slate-500">
            <span className="font-medium text-slate-700">{records.length}</span>{' '}
            record{records.length === 1 ? '' : 's'} · Latest activity:{' '}
            <span className="font-medium text-slate-700">
              {latestRecordDate}
            </span>
          </p>
        </header>

        <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-100 p-5">
            <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
              <div>
                <h2 className="text-lg font-semibold text-slate-900">
                  Access history
                </h2>

                <p className="mt-1 text-sm text-slate-500">
                  Filter records by action type or company name.
                </p>
              </div>

              {records.length > 0 && (
                <div className="grid w-full gap-3 lg:w-auto lg:grid-cols-[180px_260px]">
                  <div>
                    <label className="mb-1 block text-sm font-medium text-slate-700">
                      Action
                    </label>

                    <select
                      value={selectedAction}
                      onChange={(event) =>
                        handleActionChange(event.target.value)
                      }
                      className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-100"
                    >
                      {actionOptions.map((action) => (
                        <option key={action} value={action}>
                          {action === 'All' ? 'All actions' : action}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="mb-1 block text-sm font-medium text-slate-700">
                      Company
                    </label>

                    <input
                      value={companySearch}
                      onChange={(event) =>
                        handleCompanySearchChange(event.target.value)
                      }
                      placeholder="Search company"
                      className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-100"
                    />
                  </div>
                </div>
              )}
            </div>
          </div>

          {records.length === 0 ? (
            <div className="p-8">
              <div className="rounded-xl border border-dashed border-slate-300 bg-slate-50 p-8 text-center">
                <h3 className="text-base font-semibold text-slate-900">
                  No audit records found
                </h3>

                <p className="mt-2 text-sm text-slate-500">
                  Access-related blockchain records will appear here when
                  records have been created.
                </p>
              </div>
            </div>
          ) : filteredRecords.length === 0 ? (
            <div className="p-8">
              <div className="rounded-xl border border-dashed border-slate-300 bg-slate-50 p-8 text-center">
                <h3 className="text-base font-semibold text-slate-900">
                  No records match this filter
                </h3>

                <p className="mt-2 text-sm text-slate-500">
                  Try changing the action or company filter.
                </p>
              </div>
            </div>
          ) : (
            <>
              <div className="divide-y divide-slate-100">
                {pagedRecords.map((record) => {
                  const tone = getActionTone(record.action);
                  const key =
                    record.transactionHash ||
                    `${record.action}-${record.createdAt}-${record.employerName}`;

                  const isExpanded = expandedKey === key;
                  const isRequestReviewed = record.action === 'RequestReviewed';
                  const recordTypeLabel = getRecordTypeLabel(record);

                  return (
                    <article key={key} className="px-6 py-5">
                      <div className="grid gap-4 lg:grid-cols-[150px_1fr_auto] lg:items-start">
                        <div className="text-sm text-slate-500">
                          {formatDateTime(record.createdAt)}
                        </div>

                        <div className="relative min-w-0 pl-6">
                          <span
                            className={`absolute left-0 top-1.5 h-3 w-3 rounded-full ${tone.dotClass}`}
                          />

                          <div className="flex flex-wrap items-center gap-2">
                            <span
                              className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ${tone.badgeClass}`}
                            >
                              {tone.label}
                            </span>

                            <h3 className="text-base font-semibold text-slate-900">
                              {getDisplayTitle(record)}
                            </h3>
                          </div>

                          <p className="mt-1 text-sm text-slate-600">
                            {getDisplayMessage(record)}
                          </p>

                          <p className="mt-2 text-sm text-slate-500">
                            Company:{' '}
                            <span className="font-medium text-slate-800">
                              {record.employerName || 'Unknown company'}
                            </span>
                            <span className="mx-2 text-slate-300">·</span>
                            Type:{' '}
                            <span className="font-medium text-slate-800">
                              {recordTypeLabel}
                            </span>
                            <span className="mx-2 text-slate-300">·</span>
                            Proof:{' '}
                            <span className="font-mono text-slate-700">
                              {shortenValue(record.transactionHash)}
                            </span>
                          </p>

                          {isExpanded && (
                            <div className="mt-4 rounded-xl border border-slate-200 bg-slate-50 p-4">
                              <h4 className="text-sm font-semibold text-slate-900">
                                {isRequestReviewed
                                  ? 'Request review details'
                                  : 'Access details'}
                              </h4>

                              <div className="mt-4 grid gap-4 lg:grid-cols-2">
                                <div>
                                  <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                                    Company
                                  </p>
                                  <p className="mt-1 text-sm font-medium text-slate-800">
                                    {record.employerName || 'Unknown company'}
                                  </p>
                                </div>

                                <div>
                                  <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                                    Type
                                  </p>
                                  <p className="mt-1 text-sm font-medium text-slate-800">
                                    {recordTypeLabel}
                                  </p>
                                </div>
                              </div>

                              <div className="mt-5">
                                <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                                  {actionItemHeading(record.action)}
                                </p>

                                {record.itemLabels.length > 0 ? (
                                  <div className="mt-2 grid gap-3">
                                    {record.itemLabels.map((item) => {
                                      const formatted = isRequestReviewed
                                        ? formatReviewSummaryItem(item)
                                        : {
                                          heading: '',
                                          body: item,
                                        };

                                      return (
                                        <div
                                          key={item}
                                          className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-800"
                                        >
                                          {formatted.heading && (
                                            <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-slate-400">
                                              {formatted.heading}
                                            </p>
                                          )}
                                          <p>{formatted.body}</p>
                                        </div>
                                      );
                                    })}
                                  </div>
                                ) : (
                                  <p className="mt-2 text-sm text-slate-500">
                                    No specific items were attached to this
                                    record.
                                  </p>
                                )}
                              </div>

                              <p className="mt-5 border-t border-slate-200 pt-4 text-xs text-slate-500">
                                Blockchain proof:{' '}
                                <span className="font-mono">
                                  {shortenValue(record.transactionHash, 12, 10)}
                                </span>
                              </p>
                            </div>
                          )}
                        </div>

                        <button
                          onClick={() =>
                            setExpandedKey(isExpanded ? null : key)
                          }
                          className="w-fit rounded-lg border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50"
                        >
                          {isExpanded ? 'Hide details' : 'Details'}
                        </button>
                      </div>
                    </article>
                  );
                })}
              </div>

              {filteredRecords.length > PAGE_SIZE && (
                <div className="flex items-center justify-between border-t border-slate-100 px-6 py-4">
                  <p className="text-sm text-slate-500">
                    Showing {(page - 1) * PAGE_SIZE + 1}-
                    {Math.min(page * PAGE_SIZE, filteredRecords.length)} of{' '}
                    {filteredRecords.length}
                  </p>

                  <div className="flex gap-2">
                    <button
                      disabled={page === 1}
                      onClick={handlePreviousPage}
                      className="rounded-lg border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
                    >
                      Previous
                    </button>

                    <span className="rounded-lg bg-slate-100 px-3 py-2 text-sm font-medium text-slate-700">
                      Page {page} of {totalPages}
                    </span>

                    <button
                      disabled={page === totalPages}
                      onClick={handleNextPage}
                      className="rounded-lg border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
                    >
                      Next
                    </button>
                  </div>
                </div>
              )}
            </>
          )}
        </section>
      </div>
    </main>
  );
}
