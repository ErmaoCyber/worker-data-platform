'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { getWorkerDashboard } from '@/lib/workerDashboardApi';
import WorkerDashboardView from './WorkerDashboardView';
import type { WorkerDashboardResponse } from '@/lib/workerDashboardApi';
import { useAuth } from '@/context/AuthContext';

export default function WorkerDashboardPage() {
  const router = useRouter();

  const [data, setData] = useState<WorkerDashboardResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const { token, role, isAuthReady } = useAuth();

  useEffect(() => {
    async function loadDashboard() {
      if (!isAuthReady) {
        return;
      }

      if (!token || role !== 'worker') {
        router.push('/login');
        return;
      }

      try {
        setLoading(true);
        setError('');

        const dashboardData = await getWorkerDashboard(token);
        setData(dashboardData);
      } catch {
        setError('Failed to load dashboard.');
      } finally {
        setLoading(false);
      }
    }

    loadDashboard();
  }, [isAuthReady, token, role, router]);

  if (!isAuthReady || loading) {
    return (
      <main className="min-h-screen bg-slate-50 px-8 py-8">
        <p className="text-slate-600">Loading dashboard...</p>
      </main>
    );
  }

  if (error) {
    return (
      <main className="min-h-screen bg-slate-50 px-8 py-8">
        <p className="text-red-600">{error}</p>
      </main>
    );
  }

  if (!data) {
    return (
      <main className="min-h-screen bg-slate-50 px-8 py-8">
        <p className="text-slate-600">No dashboard data found.</p>
      </main>
    );
  }

  return <WorkerDashboardView data={data} />;
}
