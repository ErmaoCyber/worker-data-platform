'use client';
import { AuthProvider } from '@/context/AuthContext';
import { NotificationRefreshProvider } from '@/context/NotificationRefreshContext';

export default function Providers({ children }: { children: React.ReactNode }) {
    return (
        <AuthProvider>
            <NotificationRefreshProvider>{children}</NotificationRefreshProvider>
        </AuthProvider>
    );
}
