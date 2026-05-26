import Sidebar from "@/component/sidebar/Sidebar";

export default function WorkerLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex h-screen">
      <Sidebar />

      <main className="flex-1 overflow-y-auto">
        {children}
      </main>
    </div>
  );
}
