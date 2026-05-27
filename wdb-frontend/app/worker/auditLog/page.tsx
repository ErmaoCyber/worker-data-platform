'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import {
  getMyWorkerAuditLog,
  type AuditLogRecord,
} from '@/lib/workerAuditLogApi';
import AuditLogView from './AuditLogView';

export default function WorkerAuditLogPage() {
  const router = useRouter();

  const { token, role, isAuthReady } = useAuth();

  const [records, setRecords] = useState<AuditLogRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadAuditLog() {
      if (!isAuthReady) {
        return;
      }

      // Only logged-in workers can view their own audit log.
      if (!token || role !== 'worker') {
        router.push('/login');
        return;
      }

      try {
        setLoading(true);
        setError('');

        const result = await getMyWorkerAuditLog(token);
        setRecords(result.records);
      } catch {
        setError('Could not load audit log. Please try again later.');
      } finally {
        setLoading(false);
      }
    }

    loadAuditLog();
  }, [isAuthReady, token, role, router]);

  if (!isAuthReady || loading) {
    return (
      <main className="p-6">
        <p className="text-sm text-gray-600">Loading audit log...</p>
      </main>
    );
  }

  if (error) {
    return (
      <main className="p-6">
        <section className="rounded-lg border border-red-200 bg-red-50 p-6">
          <h1 className="text-lg font-medium text-red-700">
            Audit log could not be loaded
          </h1>
          <p className="mt-2 text-sm text-red-600">{error}</p>
        </section>
      </main>
    );
  }

  return <AuditLogView records={records} />;
}
