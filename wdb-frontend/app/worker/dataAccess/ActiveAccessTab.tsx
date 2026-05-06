"use client";

import { useEffect, useState } from "react";
import ActiveRow, { ActiveRowData } from "../../components/ActiveRow";
import { FetchApi } from "../../../lib/api";

export default function ActiveAccessTab() {
    const [workerId, setWorkerId] = useState('');
    const [permissions, setPermissions] = useState<ActiveRowData[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [refreshTrigger, setRefreshTrigger] = useState(0);

    useEffect(() => {
        const id = localStorage.getItem('userId');
        if (id) setWorkerId(id);
    }, []);

    useEffect(() => {
        if (!workerId) return;
        setIsLoading(true);
        FetchApi(`/api/Worker/${workerId}/active-access`)
            .then((data) =>
                setPermissions(
                    data.map((item: any) => ({
                        id: item.id,
                        company: item.company,
                        date: item.date,
                        reason: item.reason,
                        workerInfo: item.workerInfo,
                    }))
                )
            )
            .finally(() => setIsLoading(false));
    }, [workerId, refreshTrigger]);

    const handleRevoke = (itemId: string, _workerInfoIds: string[]) => {
        FetchApi(`/api/Permission/${itemId}/reject`)
            .then(() => setRefreshTrigger((n) => n + 1));
    };

    if (isLoading) return <p className="text-sm text-gray-500">Loading...</p>;
    if (permissions.length === 0) return <p className="text-sm text-gray-500">No active access grants.</p>;

    return (
        <div>
            {permissions.map((item) => (
                <ActiveRow key={item.id} {...item} onRevoke={handleRevoke} />
            ))}
        </div>
    );
}
