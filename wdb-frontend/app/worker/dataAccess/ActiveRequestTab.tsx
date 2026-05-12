"use client";

import RequestRow, { Row } from "../../components/RequestRow"
import { useState, useEffect } from "react";
import { FetchApi } from '../../../lib/api';
import { useAuth } from "@/context/AuthContext";

export default function ActiveRequestTab() {
    const { token } = useAuth(); // get the token from auth context
    const [rows, setRows] = useState<Row[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [errorMsg, setErrorMsg] = useState('');
    const [refreshTrigger, setRefreshTrigger] = useState(0);

    useEffect(() => {
        getRows(token); // fetch the active requests when the component mounts or when refreshTrigger changes
    }, [refreshTrigger, token]);

    async function getRows(token: string | null) {
        setIsLoading(true);
        setErrorMsg('');
        try {
            var rows = await FetchApi(
                `/api/Worker/rows`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });
            if (!rows) {
                alert("There are no current requests");
                return;
            }
            setRows(rows);
        } catch (error) {
            setErrorMsg(`${error}`)
        } finally {
            setIsLoading(false);
        }
    }

    const handleComplete = () => {
        setRefreshTrigger(prev => prev + 1);
    };

    if (!isLoading && rows.length === 0) {
        return <p className="text-sm text-gray-500">No active permission requests</p>;
    }

    return (
        <div>
            {isLoading && <p className="text-sm text-gray-500">Loading...</p>}
            {errorMsg && <p className="text-sm text-red-500">{errorMsg}</p>}
            {!isLoading && rows.map((item) => (
                <RequestRow key={item.id} {...item} onComplete={handleComplete} />
            ))}

        </div>
    );
}