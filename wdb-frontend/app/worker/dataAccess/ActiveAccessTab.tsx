'use client';

import { useEffect, useMemo, useState } from 'react';
import {
  getWorkerActiveAccess,
  revokeWorkerActiveAccess,
  type ActiveAccessInfo,
  type ActiveAccessRecord,
} from '@/lib/workerDataAccessApi';

interface ActiveAccessTabProps {
  token: string;
  refreshKey: number;
  onChanged: () => void;
}

type CategoryGroup = {
  category: string;
  categoryLabel: string;
  items: ActiveAccessInfo[];
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

function groupItemsByCategory(items: ActiveAccessInfo[]): CategoryGroup[] {
  const groups = new Map<string, CategoryGroup>();

  items.forEach((item) => {
    const category = item.category || 'OtherInformation';
    const categoryLabel = item.categoryLabel || 'Other Information';

    if (!groups.has(category)) {
      groups.set(category, {
        category,
        categoryLabel,
        items: [],
      });
    }

    groups.get(category)!.items.push(item);
  });

  return Array.from(groups.values())
    .map((group) => ({
      ...group,
      items: group.items.sort((a, b) => a.dataType.localeCompare(b.dataType)),
    }))
    .sort((a, b) => a.categoryLabel.localeCompare(b.categoryLabel));
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
  const [selectedCategory, setSelectedCategory] = useState('All');

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

  const categoryOptions = useMemo(() => {
    const categories = records.flatMap((record) =>
      record.workerInfo.map((info) => ({
        value: info.category,
        label: info.categoryLabel,
      })),
    );

    const unique = new Map<string, string>();

    categories.forEach((category) => {
      if (category.value && !unique.has(category.value)) {
        unique.set(category.value, category.label || category.value);
      }
    });

    return [
      { value: 'All', label: 'All categories' },
      ...Array.from(unique.entries())
        .map(([value, label]) => ({ value, label }))
        .sort((a, b) => a.label.localeCompare(b.label)),
    ];
  }, [records]);

  const filteredRecords = useMemo(() => {
    return records
      .map((record) => {
        const filteredInfo =
          selectedCategory === 'All'
            ? record.workerInfo
            : record.workerInfo.filter(
              (info) => info.category === selectedCategory,
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
  }, [records, searchText, selectedCategory]);

  async function handleRevokeRequest(record: ActiveAccessRecord) {
    const confirmed = window.confirm(
      `Revoke all access for ${record.companyName || 'this company'}? The company will no longer be allowed to access the approved items in this grant.`,
    );

    if (!confirmed) return;

    setActionId(record.requestId);
    setErrorMsg('');

    try {
      await revokeWorkerActiveAccess(token, record.requestId);
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
                Category
              </label>
              <select
                value={selectedCategory}
                onChange={(event) => setSelectedCategory(event.target.value)}
                className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-100"
              >
                {categoryOptions.map((category) => (
                  <option key={category.value} value={category.value}>
                    {category.label}
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
            {filteredRecords.map((record) => {
              const isRevoking = actionId === record.requestId;
              const categoryGroups = groupItemsByCategory(record.workerInfo);
              const totalItems = record.workerInfo.length;

              return (
                <div
                  key={record.requestId}
                  className="rounded-xl border border-slate-200 bg-slate-50/50 p-5"
                >
                  <div className="flex flex-col gap-3 border-b border-slate-200 pb-4 sm:flex-row sm:items-start sm:justify-between">
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

                    <div className="flex flex-col gap-2 sm:items-end">
                      <span className="w-fit rounded-full bg-emerald-50 px-3 py-1 text-xs font-medium text-emerald-700">
                        Active
                      </span>

                      <button
                        disabled={isRevoking}
                        onClick={() => handleRevokeRequest(record)}
                        className="w-fit rounded-lg border border-red-200 bg-white px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-40"
                      >
                        {isRevoking ? 'Revoking...' : 'Revoke access'}
                      </button>
                    </div>
                  </div>

                  <div className="mt-4">
                    <div className="mb-3 flex items-center justify-between">
                      <h4 className="text-sm font-semibold text-slate-900">
                        Included categories
                      </h4>
                      <span className="rounded-full bg-white px-2 py-1 text-xs text-slate-500">
                        {categoryGroups.length} categor
                        {categoryGroups.length === 1 ? 'y' : 'ies'} ·{' '}
                        {totalItems} item{totalItems === 1 ? '' : 's'}
                      </span>
                    </div>

                    <div className="grid gap-3">
                      {categoryGroups.map((group) => (
                        <div
                          key={group.category}
                          className="rounded-xl border border-slate-200 bg-white p-4"
                        >
                          <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                            <div>
                              <h5 className="font-medium text-slate-900">
                                {group.categoryLabel}
                              </h5>
                              <p className="mt-1 text-sm text-slate-500">
                                {group.items.length} item
                                {group.items.length === 1 ? '' : 's'} included
                              </p>
                            </div>

                            <span className="w-fit rounded-full bg-slate-100 px-2 py-1 text-xs text-slate-600">
                              Access granted
                            </span>
                          </div>

                          <div className="mt-3 flex flex-wrap gap-2">
                            {group.items.map((item) => (
                              <span
                                key={item.permissionId}
                                className="rounded-full border border-slate-200 bg-slate-50 px-3 py-1 text-sm text-slate-700"
                              >
                                {item.dataType}
                              </span>
                            ))}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </section>
  );
}
