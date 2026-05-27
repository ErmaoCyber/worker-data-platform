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

const PAGE_SIZE = 6;

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

  const [searchText, setSearchText] = useState('');
  const [selectedStatus, setSelectedStatus] = useState('All');
  const [selectedDataType, setSelectedDataType] = useState('All');
  const [currentPage, setCurrentPage] = useState(1);

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

  useEffect(() => {
    setCurrentPage(1);
  }, [searchText, selectedStatus, selectedDataType]);

  const statusOptions = [
    'All',
    'Pending',
    'Approved',
    'Rejected',
    'Partial',
    'Expired',
  ];

  const dataTypes = useMemo(() => {
    const allTypes = requests.flatMap((request) => request.requestedDataTypes);
    return ['All', ...Array.from(new Set(allTypes))];
  }, [requests]);

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

  const totalPages = Math.max(1, Math.ceil(filteredRequests.length / PAGE_SIZE));

  const paginatedRequests = filteredRequests.slice(
    (currentPage - 1) * PAGE_SIZE,
    currentPage * PAGE_SIZE
  );

  if (isLoading) {
    return <p className="text-sm text-gray-500">Loading requests...</p>;
  }

  if (errorMsg) {
    return <p className="text-sm text-red-500">{errorMsg}</p>;
  }

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
        <h2 className="text-lg font-semibold text-gray-900">
          My Requests
        </h2>
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
              onChange={(event) => setSearchText(event.target.value)}
              placeholder="Search by name or email"
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder:text-gray-400 focus:border-gray-900 focus:outline-none"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Status
            </label>
            <select
              value={selectedStatus}
              onChange={(event) => setSelectedStatus(event.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 focus:border-gray-900 focus:outline-none"
            >
              {statusOptions.map((status) => (
                <option key={status} value={status}>
                  {status === 'All' ? 'All statuses' : status}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Data type
            </label>
            <select
              value={selectedDataType}
              onChange={(event) => setSelectedDataType(event.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 focus:border-gray-900 focus:outline-none"
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

      {filteredRequests.length === 0 ? (
        <p className="text-sm text-gray-500">
          No requests match your filter.
        </p>
      ) : (
        <>
          <div className="rounded-xl border border-gray-200 bg-white">
            <div className="divide-y divide-gray-200">
              {paginatedRequests.map((request) => (
                <div
                  key={request.requestId}
                  className="grid gap-4 p-4 md:grid-cols-[1.2fr_1.5fr_1.5fr_8rem_8rem]"
                >
                  <div>
                    <p className="text-sm text-gray-500">Worker</p>
                    <p className="mt-1 font-medium text-gray-900">
                      {request.workerName}
                    </p>
                    <p className="mt-1 text-xs text-gray-500">
                      {request.workerEmail}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Requested Data</p>
                    <p className="mt-1 text-sm text-gray-700">
                      {request.requestedDataTypes.join(', ')}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Reason</p>
                    <p className="mt-1 text-sm text-gray-700">
                      {request.reason}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Status</p>
                    <span
                      className={`mt-1 inline-flex w-24 justify-center rounded-full border px-3 py-1 text-xs font-medium ${getStatusClassName(
                        request.status
                      )}`}
                    >
                      {request.status}
                    </span>
                  </div>

                  <div className="md:text-right">
                    <p className="text-sm text-gray-500">Requested</p>
                    <p className="mt-1 text-sm text-gray-600">
                      {new Date(request.requestedAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>
              ))}
            </div>
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
                onClick={() => setCurrentPage((page) => page - 1)}
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
                onClick={() => setCurrentPage((page) => page + 1)}
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
