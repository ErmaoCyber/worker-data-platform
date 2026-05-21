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

export default function MyAccessTab() {
  const router = useRouter();

  const [accessList, setAccessList] = useState<EmployerActiveAccess[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  // Filter states
  const [searchText, setSearchText] = useState('');
  const [selectedDataType, setSelectedDataType] = useState('All');

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

  // Build data type options from approved access records.
  const dataTypes = useMemo(() => {
    const allTypes = accessList.flatMap((access) =>
      access.workerInfo.map((info) => info.dataType)
    );

    return ['All', ...Array.from(new Set(allTypes))];
  }, [accessList]);

  // Apply worker search and data type filter.
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

  if (isLoading) {
    return <p className="text-sm text-slate-500">Loading active access...</p>;
  }

  if (errorMsg) {
    return <p className="text-sm text-red-600">{errorMsg}</p>;
  }

  if (accessList.length === 0) {
    return (
      <div className="rounded-2xl border border-slate-200 bg-white p-6 text-sm text-slate-500">
        No active access found.
      </div>
    );
  }

  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
      <h2 className="text-lg font-semibold text-slate-900">
        My Access
      </h2>

      <p className="mt-1 mb-4 text-sm text-slate-500">
        Worker data your company is currently approved to access.
      </p>

      {/* Filter area */}
      <div className="mb-5 rounded-xl border border-slate-200 bg-slate-50 p-4">
        <div className="grid gap-3 md:grid-cols-2">
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

      {/* Filtered active access list */}
      {filteredAccessList.length === 0 ? (
        <p className="rounded-xl border border-slate-200 bg-slate-50 p-4 text-sm text-slate-500">
          No active access matches your filter.
        </p>
      ) : (
        <div className="space-y-4">
          {filteredAccessList.map((access) => (
            <div
              key={access.requestId}
              className="rounded-xl border border-slate-200 bg-white p-4"
            >
              <div className="flex flex-col gap-2 border-b border-slate-100 pb-3 md:flex-row md:items-start md:justify-between">
                <div>
                  <p className="text-sm text-slate-500">Worker</p>
                  <p className="mt-1 font-medium text-slate-900">
                    {access.workerName}
                  </p>
                  <p className="mt-1 text-xs text-slate-500">
                    {access.workerEmail}
                  </p>
                </div>

                <div className="md:text-right">
                  <p className="text-sm text-slate-500">Granted</p>
                  <p className="mt-1 text-sm text-slate-600">
                    {new Date(access.grantedAt).toLocaleDateString()}
                  </p>

                  <p className="mt-2 text-sm text-slate-500">Expiry</p>
                  <p className="mt-1 text-sm text-slate-600">
                    {access.expiryDate
                      ? new Date(access.expiryDate).toLocaleDateString()
                      : 'No expiry date'}
                  </p>
                </div>
              </div>

              <div className="mt-3">
                <p className="text-sm text-slate-500">Reason</p>
                <p className="mt-1 text-sm text-slate-700">
                  {access.reason}
                </p>
              </div>

              <div className="mt-4 grid gap-3 md:grid-cols-2">
                {access.workerInfo.map((info) => (
                  <div
                    key={info.permissionId}
                    className="rounded-lg border border-slate-200 bg-slate-50 p-3"
                  >
                    <p className="text-xs font-medium uppercase tracking-wide text-slate-500">
                      {info.dataType}
                    </p>
                    <p className="mt-1 text-sm text-slate-900">
                      {info.value}
                    </p>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
