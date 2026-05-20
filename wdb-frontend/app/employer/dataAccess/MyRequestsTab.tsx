'use client';

import { useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { FetchApi } from '@/lib/api';

type EmployerSentRequest = {
  requestId: string;
  workerId: string;
  workerName: string;
  workerEmail: string;
  reason: string;
  requestedAt: string;
  lastUpdatedAt: string;
  status: string;
  requestedDataTypes: string[];
};

function getStatusClassName(status: string) {
  switch (status) {
    case 'Pending':
      return 'border-amber-200 bg-amber-50 text-amber-700';
    case 'Approved':
      return 'border-emerald-200 bg-emerald-50 text-emerald-700';
    case 'Rejected':
      return 'border-red-200 bg-red-50 text-red-700';
    case 'Expired':
      return 'border-slate-200 bg-slate-100 text-slate-600';
    case 'Partial':
      return 'border-blue-200 bg-blue-50 text-blue-700';
    default:
      return 'border-slate-200 bg-slate-50 text-slate-600';
  }
}

export default function MyRequestsTab() {
  const router = useRouter();

  const [requests, setRequests] = useState<EmployerSentRequest[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  // Filter states
  const [searchText, setSearchText] = useState('');
  const [selectedStatus, setSelectedStatus] = useState('All');
  const [selectedDataType, setSelectedDataType] = useState('All');

  useEffect(() => {
    async function loadRequests() {
      const token = localStorage.getItem('accessToken');
      const role = localStorage.getItem('role');

      if (!token || role !== 'employer') {
        router.push('/login');
        return;
      }

      setIsLoading(true);
      setErrorMsg('');

      try {
        const data: EmployerSentRequest[] = await FetchApi(
          '/api/Employer/sent-requests',
          {
            method: 'GET',
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );

        setRequests(data);
      } catch {
        setErrorMsg('Failed to load sent requests.');
      } finally {
        setIsLoading(false);
      }
    }

    loadRequests();
  }, [router]);

  // Status options are based on backend status values.
  const statusOptions = [
    'All',
    'Pending',
    'Approved',
    'Rejected',
    'Partial',
    'Expired',
  ];

  // Build data type dropdown options from the request data.
  const dataTypes = useMemo(() => {
    const allTypes = requests.flatMap((request) => request.requestedDataTypes);
    return ['All', ...Array.from(new Set(allTypes))];
  }, [requests]);

  // Apply worker search, status filter, and data type filter.
  const filteredRequests = useMemo(() => {
    return requests.filter((request) => {
      const search = searchText.toLowerCase();

      const matchesWorker =
        request.workerName.toLowerCase().includes(search) ||
        request.workerEmail.toLowerCase().includes(search);

      const matchesStatus =
        selectedStatus === 'All' || request.status === selectedStatus;

      const matchesDataType =
        selectedDataType === 'All' ||
        request.requestedDataTypes.includes(selectedDataType);

      return matchesWorker && matchesStatus && matchesDataType;
    });
  }, [requests, searchText, selectedStatus, selectedDataType]);

  if (isLoading) {
    return <p className="text-sm text-slate-500">Loading requests...</p>;
  }

  if (errorMsg) {
    return <p className="text-sm text-red-600">{errorMsg}</p>;
  }

  if (requests.length === 0) {
    return (
      <div className="rounded-2xl border border-slate-200 bg-white p-6 text-sm text-slate-500">
        No sent requests found.
      </div>
    );
  }

  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
      <h2 className="text-lg font-semibold text-slate-900">
        My Requests
      </h2>

      <p className="mt-1 mb-4 text-sm text-slate-500">
        All data access requests sent by your company.
      </p>

      {/* Filter area */}
      <div className="mb-5 rounded-xl border border-slate-200 bg-slate-50 p-4">
        <div className="grid gap-3 md:grid-cols-3">
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Search worker
            </label>
            <input
              type="text"
              value={searchText}
              onChange={(event) => setSearchText(event.target.value)}
              placeholder="Search by name or email"
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-slate-900 focus:outline-none"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Status
            </label>
            <select
              value={selectedStatus}
              onChange={(event) => setSelectedStatus(event.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-slate-900 focus:outline-none"
            >
              {statusOptions.map((status) => (
                <option key={status} value={status}>
                  {status === 'All' ? 'All statuses' : status}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Data type
            </label>
            <select
              value={selectedDataType}
              onChange={(event) => setSelectedDataType(event.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-slate-900 focus:outline-none"
            >
              {dataTypes.map((type) => (
                <option key={type} value={type}>
                  {type === 'All' ? 'All data types' : type}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Filtered request list */}
      {filteredRequests.length === 0 ? (
        <p className="rounded-xl border border-slate-200 bg-slate-50 p-4 text-sm text-slate-500">
          No requests match your filter.
        </p>
      ) : (
        <div className="divide-y divide-slate-200">
          {filteredRequests.map((request) => (
            <div
              key={request.requestId}
              className="grid gap-4 py-4 first:pt-0 last:pb-0 md:grid-cols-[1.2fr_1.5fr_1.5fr_8rem_8rem]"
            >
              <div>
                <p className="text-sm text-slate-500">Worker</p>
                <p className="mt-1 font-medium text-slate-900">
                  {request.workerName}
                </p>
                <p className="mt-1 text-xs text-slate-500">
                  {request.workerEmail}
                </p>
              </div>

              <div>
                <p className="text-sm text-slate-500">Requested Data</p>
                <p className="mt-1 text-sm text-slate-700">
                  {request.requestedDataTypes.join(', ')}
                </p>
              </div>

              <div>
                <p className="text-sm text-slate-500">Reason</p>
                <p className="mt-1 text-sm text-slate-700">
                  {request.reason}
                </p>
              </div>

              <div>
                <p className="text-sm text-slate-500">Status</p>
                <span
                  className={`mt-1 inline-flex w-24 justify-center rounded-full border px-3 py-1 text-xs font-medium ${getStatusClassName(
                    request.status
                  )}`}
                >
                  {request.status}
                </span>
              </div>

              <div className="md:text-right">
                <p className="text-sm text-slate-500">Requested</p>
                <p className="mt-1 text-sm text-slate-600">
                  {new Date(request.requestedAt).toLocaleDateString()}
                </p>
              </div>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
