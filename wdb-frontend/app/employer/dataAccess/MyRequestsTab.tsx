'use client';

import { useEffect, useState } from 'react';
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

      <div className="divide-y divide-slate-200">
        {requests.map((request) => (
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
    </section>
  );
}
