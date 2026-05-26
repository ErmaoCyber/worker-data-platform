'use client';

import { useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { FetchApi } from '@/lib/api';

type EmployerActiveAccess = {
  requestId: string;
  workerId: string;
  workerName: string;
  workerEmail: string;
  reason: string;
  grantedAt: string;
  expiryDate: string | null;
  workerInfo: {
    permissionId: string;
    dataType: string;
    value: string;
  }[];
};

const PAGE_SIZE = 6;

export default function MyAccessTab() {
  const router = useRouter();

  const [accessList, setAccessList] = useState<EmployerActiveAccess[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  const [searchText, setSearchText] = useState('');
  const [selectedDataType, setSelectedDataType] = useState('All');
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    async function loadActiveAccess() {
      const token = localStorage.getItem('accessToken');
      const role = localStorage.getItem('role');

      if (!token || role !== 'employer') {
        router.push('/login');
        return;
      }

      setIsLoading(true);
      setErrorMsg('');

      try {
        const data: EmployerActiveAccess[] = await FetchApi(
          '/api/Employer/active-access',
          {
            method: 'GET',
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );

        setAccessList(data);
      } catch {
        setErrorMsg('Failed to load active access.');
      } finally {
        setIsLoading(false);
      }
    }

    loadActiveAccess();
  }, [router]);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchText, selectedDataType]);

  const dataTypes = useMemo(() => {
    const allTypes = accessList.flatMap((access) =>
      access.workerInfo.map((info) => info.dataType)
    );

    return ['All', ...Array.from(new Set(allTypes))];
  }, [accessList]);

  const filteredAccessList = useMemo(() => {
    return accessList.filter((access) => {
      const search = searchText.toLowerCase();

      const matchesWorker =
        access.workerName.toLowerCase().includes(search) ||
        access.workerEmail.toLowerCase().includes(search);

      const matchesDataType =
        selectedDataType === 'All' ||
        access.workerInfo.some((info) => info.dataType === selectedDataType);

      return matchesWorker && matchesDataType;
    });
  }, [accessList, searchText, selectedDataType]);

  const totalPages = Math.max(
    1,
    Math.ceil(filteredAccessList.length / PAGE_SIZE)
  );

  const paginatedAccessList = filteredAccessList.slice(
    (currentPage - 1) * PAGE_SIZE,
    currentPage * PAGE_SIZE
  );

  if (isLoading) {
    return <p className="text-sm text-gray-500">Loading active access...</p>;
  }

  if (errorMsg) {
    return <p className="text-sm text-red-500">{errorMsg}</p>;
  }

  if (accessList.length === 0) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-4 text-sm text-gray-500">
        No active access found.
      </div>
    );
  }

  return (
    <div>
      <div className="mb-4">
        <h2 className="text-lg font-semibold text-gray-900">
          My Access
        </h2>
        <p className="mt-1 text-sm text-gray-500">
          Worker data your company is currently approved to access.
        </p>
      </div>

      <div className="mb-5 rounded-xl border border-gray-200 bg-white p-4">
        <div className="grid gap-3 md:grid-cols-2">
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

      {filteredAccessList.length === 0 ? (
        <p className="text-sm text-gray-500">
          No active access matches your filter.
        </p>
      ) : (
        <>
          <div className="rounded-xl border border-gray-200 bg-white">
            <div className="divide-y divide-gray-200">
              {paginatedAccessList.map((access) => (
                <div
                  key={access.requestId}
                  className="grid gap-4 p-4 md:grid-cols-[1.2fr_1.5fr_1.5fr_8rem_8rem]"
                >
                  <div>
                    <p className="text-sm text-gray-500">Worker</p>
                    <p className="mt-1 font-medium text-gray-900">
                      {access.workerName}
                    </p>
                    <p className="mt-1 text-xs text-gray-500">
                      {access.workerEmail}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Approved Data</p>
                    <div className="mt-1 space-y-1">
                      {access.workerInfo.map((info) => (
                        <p
                          key={info.permissionId}
                          className="text-sm text-gray-700"
                        >
                          <span className="font-medium">{info.dataType}:</span>{' '}
                          {info.value}
                        </p>
                      ))}
                    </div>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Reason</p>
                    <p className="mt-1 text-sm text-gray-700">
                      {access.reason}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Granted</p>
                    <p className="mt-1 text-sm text-gray-600">
                      {new Date(access.grantedAt).toLocaleDateString()}
                    </p>
                  </div>

                  <div className="md:text-right">
                    <p className="text-sm text-gray-500">Expiry</p>
                    <p className="mt-1 text-sm text-gray-600">
                      {access.expiryDate
                        ? new Date(access.expiryDate).toLocaleDateString()
                        : 'No expiry'}
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
              {Math.min(currentPage * PAGE_SIZE, filteredAccessList.length)}
              {' of '}
              {filteredAccessList.length}
              {' access records'}
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
