"use client";

import { useEffect, useMemo, useState } from "react";
import ActiveRow, { ActiveRowData } from "../../components/ActiveRow";
import { FetchApi } from "../../../lib/api";

interface Props {
  workerId: string;
}

// New backend response shape
type ActiveAccessApiItem = {
  requestId: string;
  companyName: string;
  grantedAt: string;
  reason: string;
  workerInfo: {
    permissionId: string;
    dataType: string;
  }[];
};

export default function ActiveAccessTab({ workerId }: Props) {
  const [permissions, setPermissions] = useState<ActiveRowData[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  // New filter states
  const [searchText, setSearchText] = useState('');
  const [selectedDataType, setSelectedDataType] = useState('All');

  useEffect(() => {
    if (!workerId) return;

    async function loadActiveAccess() {
      setIsLoading(true);
      setErrorMsg('');

      try {
        const data: ActiveAccessApiItem[] = await FetchApi(
          `/api/Worker/${workerId}/active-access`
        );

        // Map new backend response to the existing ActiveRowData shape
        setPermissions(
          data.map((item) => ({
            id: item.requestId,
            company: item.companyName,
            date: new Date(item.grantedAt).toLocaleString(),
            reason: item.reason,
            workerInfo: item.workerInfo.map((info) => ({
              id: info.permissionId,
              label: info.dataType,
            })),
          }))
        );
      } catch (error) {
        setErrorMsg(`${error}`);
      } finally {
        setIsLoading(false);
      }
    }

    loadActiveAccess();
  }, [workerId, refreshTrigger]);

  // Build dropdown options from the active access data
  const dataTypes = useMemo(() => {
    const allTypes = permissions.flatMap((item) =>
      item.workerInfo.map((info) => info.label)
    );

    return ['All', ...Array.from(new Set(allTypes))];
  }, [permissions]);

  // Apply company search and data type filter
  const filteredPermissions = useMemo(() => {
    return permissions.filter((item) => {
      const matchesCompany = item.company
        .toLowerCase()
        .includes(searchText.toLowerCase());

      const matchesDataType =
        selectedDataType === 'All' ||
        item.workerInfo.some((info) => info.label === selectedDataType);

      return matchesCompany && matchesDataType;
    });
  }, [permissions, searchText, selectedDataType]);

  // Revoke logic unchanged
  const handleRevoke = (itemId: string, workerInfoIds: string[]) => {
    Promise.all(
      workerInfoIds.map((permissionId) =>
        FetchApi(`/api/Permission/${permissionId}/reject`, {
          method: "PATCH",
        })
      )
    ).then(() => setRefreshTrigger((n) => n + 1));
  };

  if (isLoading) {
    return <p className="text-sm text-gray-500">Loading...</p>;
  }

  if (errorMsg) {
    return <p className="text-sm text-red-500">{errorMsg}</p>;
  }

  if (permissions.length === 0) {
    return <p className="text-sm text-gray-500">No active access grants.</p>;
  }

  return (
    <div className="space-y-4">
      {/* Filter area */}
      <div className="rounded-xl border border-gray-200 bg-white p-4">
        <div className="grid gap-3 md:grid-cols-2">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Search company
            </label>
            <input
              type="text"
              value={searchText}
              onChange={(event) => setSearchText(event.target.value)}
              placeholder="Search by company name"
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

      {/* Filtered active access list */}
      {filteredPermissions.length === 0 ? (
        <p className="text-sm text-gray-500">
          No active access matches your filter.
        </p>
      ) : (
        <div className="space-y-3">
          {filteredPermissions.map((item) => (
            <ActiveRow
              key={item.id}
              {...item}
              onRevoke={handleRevoke}
            />
          ))}
        </div>
      )}
    </div>
  );
}
