'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  getEmployerDashboardMe,
  type EmployerDashboardData,
} from '@/lib/employerDashboardApi';
import EmployerDashboardView from './EmployerDashboardView';
import { useAuth } from '@/context/AuthContext';

export default function EmployerDashboardPage() {
  const router = useRouter();

  const [data, setData] = useState<EmployerDashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const { token: accessToken, role } = useAuth(); // get the token and role from auth context

  useEffect(() => {
    async function loadDashboard() {

      if (!accessToken || role !== 'employer') {
        router.push('/login');
        return;
      }

      try {
        const dashboardData = await getEmployerDashboardMe(accessToken);
        setData(dashboardData);
      } catch {
        setError('Failed to load employer dashboard.');
      } finally {
        setLoading(false);
      }
    }

    loadDashboard();
  }, [router, accessToken, role]);

  if (loading) {
    return (
      <main className="min-h-screen bg-slate-50 px-8 py-8">
        <p className="text-slate-600">Loading employer dashboard...</p>
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

  return <EmployerDashboardView data={data} />;
}
