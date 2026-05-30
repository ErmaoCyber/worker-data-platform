'use client';

import { useEffect, useMemo, useState } from 'react';
import {
  getWorkerActiveRequests,
  submitWorkerRequestReview,
  type WorkerActiveRequest,
} from '@/lib/workerDataAccessApi';

interface ActiveRequestTabProps {
  token: string;
  refreshKey: number;
  onChanged: () => void;
}

type CustomFormState = {
  label: string;
  type: 'text' | 'file';
  value: string;
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

function formatDate(dateString: string) {
  if (!dateString) return '-';

  return new Date(dateString).toLocaleDateString('en-NZ', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

function categoryLabel(category: string) {
  switch (category) {
    case 'PersonalInformation':
      return 'Personal information';
    case 'MedicalInformation':
      return 'Medical information';
    case 'CareerInformation':
      return 'Career information';
    case 'OtherInformation':
      return 'Other information';
    default:
      return category || 'Other information';
  }
}

export default function ActiveRequestTab({
  token,
  refreshKey,
  onChanged,
}: ActiveRequestTabProps) {
  const [requests, setRequests] = useState<WorkerActiveRequest[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [actionId, setActionId] = useState<string | null>(null);
  const [errorMsg, setErrorMsg] = useState('');
  const [customForms, setCustomForms] = useState<Record<string, CustomFormState>>(
    {},
  );

  useEffect(() => {
    async function loadRequests() {
      setIsLoading(true);
      setErrorMsg('');

      try {
        const data = await getWorkerActiveRequests(token);
        setRequests(data);

        const nextForms: Record<string, CustomFormState> = {};

        data.forEach((request) => {
          if (request.customRequest) {
            nextForms[request.requestId] = {
              label: request.customRequest.description,
              type: 'text',
              value: '',
            };
          }
        });

        setCustomForms((current) => ({
          ...nextForms,
          ...current,
        }));
      } catch (error) {
        setErrorMsg(error instanceof Error ? error.message : String(error));
      } finally {
        setIsLoading(false);
      }
    }

    loadRequests();
  }, [token, refreshKey]);

  const totalPendingItems = useMemo(() => {
    return requests.reduce((total, request) => {
      const itemCount = request.items.length;
      const customCount = request.customRequest ? 1 : 0;
      return total + itemCount + customCount;
    }, 0);
  }, [requests]);

  async function reviewItem(
    requestId: string,
    permissionId: string,
    decision: 'approved' | 'rejected',
  ) {
    setActionId(permissionId);
    setErrorMsg('');

    try {
      await submitWorkerRequestReview(token, requestId, {
        items: [
          {
            permissionId,
            decision,
          },
        ],
        customRequestDecision: null,
      });

      onChanged();
    } catch (error) {
      setErrorMsg(error instanceof Error ? error.message : String(error));
    } finally {
      setActionId(null);
    }
  }

  async function reviewCustomRequest(
    requestId: string,
    decision: 'approved' | 'rejected',
  ) {
    const form = customForms[requestId];

    if (decision === 'approved') {
      if (!form?.label.trim()) {
        setErrorMsg('Please enter a label for the new information.');
        return;
      }

      if (!form?.value.trim()) {
        setErrorMsg('Please enter a value before approving this request.');
        return;
      }
    }

    setActionId(`custom-${requestId}`);
    setErrorMsg('');

    try {
      await submitWorkerRequestReview(token, requestId, {
        items: [],
        customRequestDecision:
          decision === 'approved'
            ? {
              decision: 'approved',
              label: form.label.trim(),
              type: form.type,
              value: form.value.trim(),
            }
            : {
              decision: 'rejected',
            },
      });

      onChanged();
    } catch (error) {
      setErrorMsg(error instanceof Error ? error.message : String(error));
    } finally {
      setActionId(null);
    }
  }

  function updateCustomForm(
    requestId: string,
    updates: Partial<CustomFormState>,
  ) {
    setCustomForms((current) => ({
      ...current,
      [requestId]: {
        label: current[requestId]?.label ?? '',
        type: current[requestId]?.type ?? 'text',
        value: current[requestId]?.value ?? '',
        ...updates,
      },
    }));
  }

  if (isLoading) {
    return (
      <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <p className="text-sm text-slate-500">Loading active requests...</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="text-lg font-semibold text-slate-900">
              Active Requests
            </h2>
            <p className="mt-1 text-sm text-slate-500">
              Requests waiting for your approval or rejection.
            </p>
          </div>

          <span className="rounded-full bg-slate-100 px-3 py-1 text-sm font-medium text-slate-700">
            {totalPendingItems} pending item{totalPendingItems === 1 ? '' : 's'}
          </span>
        </div>

        {errorMsg && (
          <div className="mt-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {errorMsg}
          </div>
        )}
      </section>

      {requests.length === 0 ? (
        <section className="rounded-2xl border border-dashed border-slate-300 bg-white p-8 text-sm text-slate-500">
          No active permission requests.
        </section>
      ) : (
        requests.map((request) => (
          <section
            key={request.requestId}
            className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"
          >
            <div className="flex flex-col gap-3 border-b border-slate-100 pb-4 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <h3 className="text-base font-semibold text-slate-900">
                  {request.companyName || 'Unknown company'}
                </h3>
                <p className="mt-1 text-sm text-slate-600">
                  Purpose: {request.reason || '-'}
                </p>
                <p className="mt-1 text-xs text-slate-500">
                  Requested {formatDateTime(request.createdAt)} · Expires{' '}
                  {formatDate(request.expiryDate)}
                </p>
              </div>

              <span className="rounded-full bg-orange-100 px-3 py-1 text-xs font-medium text-orange-700">
                Pending review
              </span>
            </div>

            <div className="mt-4 space-y-3">
              {request.items.map((item) => (
                <div
                  key={item.permissionId}
                  className="rounded-xl border border-slate-200 p-4"
                >
                  <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                    <div>
                      <div className="flex flex-wrap items-center gap-2">
                        <p className="font-medium text-slate-900">
                          {item.label}
                        </p>
                        <span className="rounded-full bg-slate-100 px-2 py-1 text-xs text-slate-600">
                          {categoryLabel(item.category)}
                        </span>
                      </div>

                      <p className="mt-1 text-sm text-slate-500">
                        {item.hasValue
                          ? `Saved value: ${item.value}`
                          : item.cannotApproveReason ??
                          'No saved value available.'}
                      </p>
                    </div>

                    <div className="flex gap-2">
                      <button
                        disabled={!item.canApprove || actionId === item.permissionId}
                        onClick={() =>
                          reviewItem(
                            request.requestId,
                            item.permissionId,
                            'approved',
                          )
                        }
                        className="rounded-lg bg-emerald-600 px-3 py-2 text-sm font-semibold text-white hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-40"
                      >
                        Approve
                      </button>

                      <button
                        disabled={actionId === item.permissionId}
                        onClick={() =>
                          reviewItem(
                            request.requestId,
                            item.permissionId,
                            'rejected',
                          )
                        }
                        className="rounded-lg border border-red-200 px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-40"
                      >
                        Reject
                      </button>
                    </div>
                  </div>
                </div>
              ))}

              {request.customRequest && (
                <div className="rounded-xl border border-blue-200 bg-blue-50 p-4">
                  <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                    <div className="flex-1">
                      <div className="flex flex-wrap items-center gap-2">
                        <p className="font-medium text-slate-900">
                          New information requested
                        </p>
                        <span className="rounded-full bg-blue-100 px-2 py-1 text-xs font-medium text-blue-700">
                          Custom request
                        </span>
                      </div>

                      <p className="mt-1 text-sm text-slate-600">
                        {request.customRequest.description}
                      </p>

                      <div className="mt-4 grid gap-3 md:grid-cols-[1fr_140px_1fr]">
                        <div>
                          <label className="mb-1 block text-xs font-medium text-slate-600">
                            Label
                          </label>
                          <input
                            value={customForms[request.requestId]?.label ?? ''}
                            onChange={(event) =>
                              updateCustomForm(request.requestId, {
                                label: event.target.value,
                              })
                            }
                            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-slate-900 focus:outline-none"
                            placeholder="e.g. Site Access Card Number"
                          />
                        </div>

                        <div>
                          <label className="mb-1 block text-xs font-medium text-slate-600">
                            Type
                          </label>
                          <select
                            value={customForms[request.requestId]?.type ?? 'text'}
                            onChange={(event) =>
                              updateCustomForm(request.requestId, {
                                type: event.target.value as 'text' | 'file',
                              })
                            }
                            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-slate-900 focus:outline-none"
                          >
                            <option value="text">Text</option>
                            <option value="file">File</option>
                          </select>
                        </div>

                        <div>
                          <label className="mb-1 block text-xs font-medium text-slate-600">
                            Value
                          </label>
                          <input
                            value={customForms[request.requestId]?.value ?? ''}
                            onChange={(event) =>
                              updateCustomForm(request.requestId, {
                                value: event.target.value,
                              })
                            }
                            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-slate-900 focus:outline-none"
                            placeholder="Enter the information to share"
                          />
                        </div>
                      </div>
                    </div>

                    <div className="flex gap-2">
                      <button
                        disabled={actionId === `custom-${request.requestId}`}
                        onClick={() =>
                          reviewCustomRequest(request.requestId, 'approved')
                        }
                        className="rounded-lg bg-emerald-600 px-3 py-2 text-sm font-semibold text-white hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-40"
                      >
                        Add & Approve
                      </button>

                      <button
                        disabled={actionId === `custom-${request.requestId}`}
                        onClick={() =>
                          reviewCustomRequest(request.requestId, 'rejected')
                        }
                        className="rounded-lg border border-red-200 bg-white px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-40"
                      >
                        Reject
                      </button>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </section>
        ))
      )}
    </div>
  );
}
