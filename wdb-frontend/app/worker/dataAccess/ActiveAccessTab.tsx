'use client';

import { useEffect, useMemo, useState } from 'react';
import {
  getWorkerActiveAccess,
  revokeWorkerActiveAccess,
  type ActiveAccessRecord,
} from '@/lib/workerDataAccessApi';

interface ActiveAccessTabProps {
  token: string;
  refreshKey: number;
  onChanged: () => void;
}

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

export default function ActiveAccessTab({
  token,
  refreshKey,
  onChanged,
}: ActiveAccessTabProps) {
  const [records, setRecords] = useState<ActiveAccessRecord[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [actionId, setActionId] = useState<string | null>(null);
  const [errorMsg, setErrorMsg] = useState('');

  const [searchText, setSearchText] = useState('');
  const [selectedDataType, setSelectedDataType] = useState('All');

  useEffect(() => {
    async function loadActiveAccess() {
      setIsLoading(true);
      setErrorMsg('');

      try {
        const data = await getWorkerActiveAccess(token);
        setRecords(data);
      } catch (error) {
        setErrorMsg(error instanceof Error ? error.message : String(error));
      } finally {
        setIsLoading(false);
      }
    }

    loadActiveAccess();
  }, [token, refreshKey]);

  const dataTypes = useMemo(() => {
    const allTypes = records.flatMap((record) =>
      record.workerInfo.map((info) => info.dataType),
    );

    return ['All', ...Array.from(new Set(allTypes))];
  }, [records]);

  const filteredRecords = useMemo(() => {
    return records
      .map((record) => {
        const filteredInfo =
          selectedDataType === 'All'
            ? record.workerInfo
            : record.workerInfo.filter(
              (info) => info.dataType === selectedDataType,
            );

        return {
          ...record,
          workerInfo: filteredInfo,
        };
      })
      .filter((record) => {
        const matchesCompany = record.companyName
          .toLowerCase()
          .includes(searchText.toLowerCase());

        return matchesCompany && record.workerInfo.length > 0;
      });
  }, [records, searchText, selectedDataType]);

  async function handleRevoke(permissionId: string, label: string) {
    const confirmed = window.confirm(
      `Revoke access to "${label}"? The company will no longer be allowed to access this item.`,
    );

    if (!confirmed) return;

    setActionId(permissionId);
    setErrorMsg('');

    try {
      await revokeWorkerActiveAccess(token, permissionId);
      onChanged();
    } catch (error) {
      setErrorMsg(error instanceof Error ? error.message : String(error));
    } finally {
      setActionId(null);
    }
  }

  if (isLoading) {
    return (
      <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <p className="text-sm text-slate-500">Loading active access...</p>
      </div>
    );
  }

  return (
    <section className="rounded-2xl border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-100 p-6">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <h2 className="text-lg font-semibold text-slate-900">
              Active Access
            </h2>
            <p className="mt-1 text-sm text-slate-500">
              Companies that currently have approved access to your data.
            </p>
          </div>

          <span className="rounded-full bg-emerald-50 px-3 py-1 text-sm font-medium text-emerald-700">
            {records.length} active grant{records.length === 1 ? '' : 's'}
          </span>
        </div>

        {errorMsg && (
          <div className="mt-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {errorMsg}
          </div>
        )}

        {records.length > 0 && (
          <div className="mt-5 grid gap-3 md:grid-cols-2">
            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700">
                Search company
              </label>
              <input
                type="text"
                value={searchText}
                onChange={(event) => setSearchText(event.target.value)}
                placeholder="Search by company name"
                className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-100"
              />
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700">
                Data type
              </label>
              <select
                value={selectedDataType}
                onChange={(event) => setSelectedDataType(event.target.value)}
                className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-100"
              >
                {dataTypes.map((type) => (
                  <option key={type} value={type}>
                    {type === 'All' ? 'All data types' : type}
                  </option>
                ))}
              </select>
            </div>
          </div>
        )}
      </div>

      <div className="p-6">
        {records.length === 0 ? (
          <div className="rounded-xl border border-dashed border-slate-300 p-8 text-sm text-slate-500">
            No active access grants.
          </div>
        ) : filteredRecords.length === 0 ? (
          <div className="rounded-xl border border-dashed border-slate-300 p-8 text-sm text-slate-500">
            No active access matches your filter.
          </div>
        ) : (
          <div className="space-y-4">
            {filteredRecords.map((record) => (
              <div
                key={record.requestId}
                className="rounded-xl border border-slate-200 bg-slate-50/50 p-5"
              >
                <div className="flex flex-col gap-2 border-b border-slate-200 pb-4 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <h3 className="text-base font-semibold text-slate-900">
                      {record.companyName || 'Unknown company'}
                    </h3>
                    <p className="mt-1 text-sm text-slate-600">
                      Purpose: {record.reason || '-'}
                    </p>
                    <p className="mt-1 text-xs text-slate-500">
                      Granted {formatDateTime(record.grantedAt)}
                    </p>
                  </div>

                  <span className="w-fit rounded-full bg-emerald-50 px-3 py-1 text-xs font-medium text-emerald-700">
                    Active
                  </span>
                </div>

                <div className="mt-4 space-y-3">
                  {record.workerInfo.map((info) => (
                    <div
                      key={info.permissionId}
                      className="flex flex-col gap-3 rounded-xl border border-slate-200 bg-white p-4 sm:flex-row sm:items-center sm:justify-between"
                    >
                      <div>
                        <p className="font-medium text-slate-900">
                          {info.dataType}
                        </p>
                        <p className="mt-1 text-sm text-slate-500">
                          This company can currently access this item.
                        </p>
                      </div>

                      <button
                        disabled={actionId === info.permissionId}
                        onClick={() =>
                          handleRevoke(info.permissionId, info.dataType)
                        }
                        className="w-fit rounded-lg border border-red-200 bg-white px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-40"
                      >
                        Revoke
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </section>
  );
}
