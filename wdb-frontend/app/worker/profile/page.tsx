'use client';

import TopBar from '@/component/ui/TopBar';
import UserInfoCard from './components/UserInfoCard';
import { useEffect, useState } from 'react';
import { WorkerInfoItem } from './type';
import BasicProfileCard from './components/BasicProfileCard';
import { getWorkerProfile, updateWorkerProfile } from '@/lib/api/workerApi';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';

// Shared placeholder component for features planned for the next stage
const PlaceholderCard = ({ title }: { title: string }) => (
  <div className="bg-white rounded-xl border border-gray-200 p-6">
    <h2 className="text-lg font-semibold text-gray-700">{title}</h2>
    <p className="text-sm text-gray-400 mt-2">Coming soon...</p>
  </div>
);

// ProfilePage fetches and updates the worker profile data.
export default function ProfilePage() {
  const router = useRouter();

  // Read auth state from AuthContext
  const {
    token,
    userId: workerId,
    userName,
    role,
    isAuthReady,
  } = useAuth();

  const [allData, setAllData] = useState<WorkerInfoItem[]>([]);

  const handleSave = async (desc: string, value: string) => {
    try {
      if (!token) {
        router.push('/login');
        return;
      }

      await updateWorkerProfile(token, desc, value);

      const updatedData = await getWorkerProfile(token);
      setAllData(Array.isArray(updatedData) ? updatedData : [updatedData]);
    } catch (error) {
      console.error('Failed to update worker profile:', error);
    }
  };

  useEffect(() => {
    async function fetchData() {
      // Wait until AuthContext finishes restoring auth data from localStorage
      if (!isAuthReady) {
        return;
      }

      // Only logged-in workers can access this page
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
      <div className="flex flex-col h-screen bg-gray-50">
        <TopBar />
        <div className="p-8 text-gray-500">Loading profile...</div>
      </div>
    );
  }

  if (!token || !workerId || !userName || role !== 'worker') {
    return null;
  }

  return (
    <div className="flex flex-col h-screen bg-gray-50">
      <TopBar />

      <div className="flex-1 overflow-y-auto px-8 py-8 flex flex-col gap-6">
        <UserInfoCard
          data={allData}
          workerId={workerId}
          userName={userName}
        />

        <BasicProfileCard
          data={allData}
          onSave={handleSave}
        />

        <PlaceholderCard title="Health Considerations" />
        <PlaceholderCard title="Emergency Contact" />
        <PlaceholderCard title="Certifications" />
        <PlaceholderCard title="Work Restrictions" />
        <PlaceholderCard title="PPE Requirements" />
      </div>
    </div>
  );
}
