'use client';

import { useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  fetchSentRequests,
  type EmployerSentRequest,
  type EmployerSentRequestItem,
} from '@/lib/api/employerSentRequestsApi';

const PAGE_SIZE = 6;

function statusLabel(s: number): string {
  switch (s) {
    case 0: return 'Pending';
    case 1: return 'Approved';
    case 2: return 'Rejected';
    case 3: return 'Revoked';
    default: return 'Unknown';
  }
}

function statusClass(s: number): string {
  switch (s) {
    case 0: return 'border-amber-200 bg-amber-50 text-amber-700';
    case 1: return 'border-emerald-200 bg-emerald-50 text-emerald-700';
    case 2: return 'border-red-200 bg-red-50 text-red-700';
    case 3: return 'border-slate-300 bg-slate-100 text-slate-600';
    default: return 'border-slate-200 bg-slate-50 text-slate-600';
  }
}

function customStatusClass(status: string | null): string {
  if (status === 'approved') return 'border-emerald-200 bg-emerald-50 text-emerald-700';
  if (status === 'rejected') return 'border-red-200 bg-red-50 text-red-700';
  return 'border-amber-200 bg-amber-50 text-amber-700';
}

// Group items by category, preserving the order each category first appears in.
function groupByCategory(
  items: EmployerSentRequestItem[],
): { name: string; items: EmployerSentRequestItem[] }[] {
  const order: string[] = [];
  const map = new Map<string, EmployerSentRequestItem[]>();
  for (const it of items) {
    if (!map.has(it.categoryName)) {
      order.push(it.categoryName);
      map.set(it.categoryName, []);
    }
    map.get(it.categoryName)!.push(it);
  }
  return order.map((name) => ({ name, items: map.get(name)! }));
}

export default function MyRequestsTab() {
  const router = useRouter();

  const [requests, setRequests] = useState<EmployerSentRequest[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  const [searchText, setSearchText] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [expandedId, setExpandedId] = useState<string | null>(null);

  // Status / data-type filters are intentionally commented out: under the
  // new schema, status is per-field rather than per-request, so the old
  // aggregate filter no longer makes sense. To be redesigned if needed.
  // const [selectedStatus, setSelectedStatus] = useState('All');
  // const [selectedDataType, setSelectedDataType] = useState('All');

  useEffect(() => {
    async function load() {
      const token = localStorage.getItem('accessToken');
      const role = localStorage.getItem('role');

      if (!token || role !== 'employer') {
        router.push('/login');
        return;
      }

      setIsLoading(true);
      setErrorMsg('');

      try {
        const data = await fetchSentRequests(token);
        setRequests(data);
      } catch {
        setErrorMsg('Failed to load sent requests.');
      } finally {
        setIsLoading(false);
      }
    }
    load();
  }, [router]);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchText]);

  const filteredRequests = useMemo(() => {
    return requests.filter((r) => {
      const search = searchText.trim().toLowerCase();
      if (!search) return true;
      return (
        r.workerName.toLowerCase().includes(search) ||
        r.workerEmail.toLowerCase().includes(search)
      );
    });
  }, [requests, searchText]);

  const totalPages = Math.max(1, Math.ceil(filteredRequests.length / PAGE_SIZE));
  const paginatedRequests = filteredRequests.slice(
    (currentPage - 1) * PAGE_SIZE,
    currentPage * PAGE_SIZE,
  );

  function toggleExpand(id: string) {
    setExpandedId((prev) => (prev === id ? null : id));
  }

  if (isLoading) return <p className="text-sm text-gray-500">Loading requests...</p>;
  if (errorMsg) return <p className="text-sm text-red-500">{errorMsg}</p>;
  if (requests.length === 0) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-4 text-sm text-gray-500">
        No sent requests found.
      </div>
    );
  }

  return (
    <div>
      <div className="mb-4">
        <h2 className="text-lg font-semibold text-gray-900">My Requests</h2>
        <p className="mt-1 text-sm text-gray-500">
          All data access requests sent by your company.
        </p>
      </div>

      <div className="mb-5 rounded-xl border border-gray-200 bg-white p-4">
        <div className="grid gap-3 md:grid-cols-3">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Search worker
            </label>
            <input
              type="text"
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              placeholder="Search by name or email"
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder:text-gray-400 focus:border-gray-900 focus:outline-none"
            />
          </div>
          {/*
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Status</label>
            <select
              value={selectedStatus}
              onChange={(e) => setSelectedStatus(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 focus:border-gray-900 focus:outline-none"
            >
              <option value="All">All statuses</option>
              <option value="Pending">Pending</option>
              <option value="Approved">Approved</option>
              <option value="Rejected">Rejected</option>
              <option value="Partial">Partial</option>
              <option value="Expired">Expired</option>
            </select>
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Data type</label>
            <select
              value={selectedDataType}
              onChange={(e) => setSelectedDataType(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 focus:border-gray-900 focus:outline-none"
            >
              <option value="All">All data types</option>
            </select>
          </div>
          */}
        </div>
      </div>

      {filteredRequests.length === 0 ? (
        <p className="text-sm text-gray-500">No requests match your filter.</p>
      ) : (
        <>
          <div className="space-y-3">
            {paginatedRequests.map((r) => {
              const isOpen = expandedId === r.requestId;
              const grouped = groupByCategory(r.items);
              const fieldCount = r.items.length;
              return (
                <div
                  key={r.requestId}
                  className="rounded-xl border border-gray-200 bg-white"
                >
                  <button
                    type="button"
                    onClick={() => toggleExpand(r.requestId)}
                    className="flex w-full items-start gap-4 p-4 text-left hover:bg-slate-50"
                  >
                    <div className="flex-1 grid gap-2 md:grid-cols-[1.2fr_1.5fr_1.5fr_8rem_8rem]">
                      <div>
                        <p className="text-sm text-gray-500">Worker</p>
                        <p className="mt-1 font-medium text-gray-900">{r.workerName}</p>
                        <p className="mt-1 text-xs text-gray-500">{r.workerEmail}</p>
                      </div>
                      <div>
                        <p className="text-sm text-gray-500">Reason</p>
                        <p className="mt-1 text-sm text-gray-700 line-clamp-2">{r.reason}</p>
                      </div>
                      <div>
                        <p className="text-sm text-gray-500">Requested</p>
                        <p className="mt-1 text-sm text-gray-700">
                          {fieldCount} field{fieldCount === 1 ? '' : 's'}
                          {r.customRequest ? ' + custom' : ''}
                        </p>
                      </div>
                      <div>
                        <p className="text-sm text-gray-500">Expires</p>
                        <p className="mt-1 text-sm text-gray-600">
                          {new Date(r.expiryDate).toLocaleDateString()}
                        </p>
                      </div>
                      <div className="md:text-right">
                        <p className="text-sm text-gray-500">Created</p>
                        <p className="mt-1 text-sm text-gray-600">
                          {new Date(r.createdAt).toLocaleDateString()}
                        </p>
                      </div>
                    </div>
                    <span className="text-slate-400 text-sm select-none">
                      {isOpen ? 'v' : '>'}
                    </span>
                  </button>

                  {isOpen && (
                    <div className="border-t border-gray-200 p-4 space-y-4">
                      {grouped.map((group) => (
                        <div key={group.name}>
                          <p className="text-sm font-semibold text-slate-800 mb-2">
                            {group.name}
                          </p>
                          <div className="space-y-1.5">
                            {group.items.map((it) => (
                              <div
                                key={it.permissionId}
                                className="flex items-center justify-between text-sm"
                              >
                                <span className="text-slate-700">
                                  {it.label}
                                  {it.isCustom && (
                                    <span className="ml-1 text-xs text-slate-400">(custom)</span>
                                  )}
                                </span>
                                <span
                                  className={`inline-flex w-20 justify-center rounded-full border px-2 py-0.5 text-xs font-medium ${statusClass(it.status)}`}
                                >
                                  {statusLabel(it.status)}
                                </span>
                              </div>
                            ))}
                          </div>
                        </div>
                      ))}

                      {r.customRequest && (
                        <div className="rounded-lg bg-slate-50 p-3">
                          <div className="flex items-center justify-between mb-1">
                            <p className="text-sm font-semibold text-slate-800">
                              Custom request
                            </p>
                            <span
                              className={`inline-flex w-20 justify-center rounded-full border px-2 py-0.5 text-xs font-medium ${customStatusClass(r.customRequestStatus)}`}
                            >
                              {r.customRequestStatus ?? 'pending'}
                            </span>
                          </div>
                          <p className="text-sm text-slate-700">{r.customRequest}</p>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              );
            })}
          </div>

          <div className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <p className="text-sm text-gray-500">
              Showing {(currentPage - 1) * PAGE_SIZE + 1}
              {' - '}
              {Math.min(currentPage * PAGE_SIZE, filteredRequests.length)}
              {' of '}
              {filteredRequests.length}
              {' requests'}
            </p>
            <div className="flex items-center gap-2">
              <button
                type="button"
                disabled={currentPage === 1}
                onClick={() => setCurrentPage((p) => p - 1)}
                className="rounded-md border border-gray-300 px-3 py-1.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Previous
              </button>
              <span className="text-sm text-gray-600">
                Page {currentPage} of {totalPages}
              </span>
              <button
                type="button"
                disabled={currentPage === totalPages}
                onClick={() => setCurrentPage((p) => p + 1)}
                className="rounded-md border border-gray-300 px-3 py-1.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Next
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
