import Sidebar from '@/component/sidebar/Sidebar';

export default function WorkerLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar role="worker" />

      <main className="flex-1 overflow-y-auto p-6">
        {children}
      </main>
    </div>
  );
}
