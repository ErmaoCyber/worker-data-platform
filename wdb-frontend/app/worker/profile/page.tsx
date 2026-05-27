'use client';
import UserInfoCard from './components/UserInfoCard';
import { useEffect, useState } from 'react';
import { WorkerInfoItem } from './type';
import BasicProfileCard from './components/BasicProfileCard';
import { getWorkerProfile, updateWorkerProfile } from '@/lib/api/workerApi';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';

const PlaceholderCard = ({ title }: { title: string }) => (
  <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
    <h2 className="text-lg font-semibold text-slate-900">{title}</h2>
    <p className="text-sm text-slate-400 mt-2">Coming soon...</p>
  </section>
);

export default function ProfilePage() {
  const router = useRouter();

  const {
    token,
    userId: workerId,
    userName,
    role,
    isAuthReady,
  } = useAuth();

  const [allData, setAllData] = useState<WorkerInfoItem[]>([]);

  const handleSave = async (desc: string, value: string, category: string) => {
    try {
      if (!token) {
        router.push('/login');
        return;
      }
      await updateWorkerProfile(token, desc, value, category);
      const updatedData = await getWorkerProfile(token);
      setAllData(Array.isArray(updatedData) ? updatedData : [updatedData]);
    } catch (error) {
      console.error('Failed to update worker profile:', error);
    }
  };

  useEffect(() => {
    async function fetchData() {
      if (!isAuthReady) return;
      if (!token || !workerId || !userName || role !== 'worker') {
        router.push('/login');
        return;
      }
      try {
        const data = await getWorkerProfile(token);
        setAllData(Array.isArray(data) ? data : [data]);
      } catch (error) {
        console.error('Failed to fetch worker profile:', error);
      }
    }
    fetchData();
  }, [isAuthReady, token, workerId, userName, role, router]);

  if (!isAuthReady) {
    return (
      <div className="flex flex-col min-h-screen bg-slate-50">
        <div className="p-8 text-slate-500">Loading profile...</div>
      </div>
    );
  }

  if (!token || !workerId || !userName || role !== 'worker') {
    return null;
  }

  return (
    <div className="flex flex-col min-h-screen bg-slate-50">

      <div className="flex-1 overflow-y-auto px-12 py-10">
        <div className="mx-auto max-w-7xl space-y-6">

          {/* Header */}
          <UserInfoCard
            data={allData}
            workerId={workerId}
            userName={userName}
          />

          {/* Basic Info */}
          <BasicProfileCard
            data={allData}
            onSave={handleSave}
          />

          {/* Coming soon - 2 column grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <PlaceholderCard title="Health Considerations" />
            <PlaceholderCard title="Emergency Contact" />
            <PlaceholderCard title="Certifications" />
            <PlaceholderCard title="Work Restrictions" />
            <PlaceholderCard title="PPE Requirements" />
          </div>

        </div>
      </div>
    </div>
  );
}
