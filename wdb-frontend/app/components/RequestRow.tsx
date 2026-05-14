"use client";

import { useState, useEffect } from "react";
import { FetchApi } from '../../lib/api';
import ConfirmModal from "./ConfirmModal";
import AddPersonalInfoModal from "./AddPersonalInfoModal"

export interface Field {
    id: string;
    label: string;
    checked: boolean;
}

export interface Row {
    id: string;
    company: string;
    date: string;
    listedInfo: Field[];
    unlistedInfo: Field[];
    reason: string;
    onComplete: () => void;
    expiryDate: string;
}


export default function RequestRow({ id, company, date, listedInfo, unlistedInfo, reason, onComplete }: Row) {
    const [checkedFields, setCheckedFields] = useState<Field[]>(listedInfo);
    const [checkedUnlistedFields, setUnlistedFields] = useState<Field[]>(unlistedInfo);
    const [errorMsg, setErrorMsg] = useState('');
    const [pendingAction, setPendingAction] = useState("");
    const [showModal, setShowModal] = useState(false);
    const [showExpiry, setShowExpiry] = useState(false);
    const [expiryDate, setExpiryDate] = useState("");
    const [showAddInfoModal, setShowAddInfoModal] = useState(false);

    const combined = [...checkedFields, ... checkedUnlistedFields];

    const toggleField = (label: string) => {
        setCheckedFields((prev) =>
            prev.map((f) => f.label === label ? { ...f, checked: !f.checked } : f)
        );
        setUnlistedFields((prev) =>
            prev.map((f) => f.label === label ? { ...f, checked: !f.checked } : f)
        );
    };


    const onExpiry = (date: string) => {
        setExpiryDate(date);
    };

    // useEffect(() => {console.log("unlisted info:", checkedUnlistedFields)}
    // )

    async function changePermission(status: "approve" | "reject") {
        const checkedIds = combined.filter((f) => f.checked).map((f) => f.id);
        try {
            await Promise.all(
                checkedIds.map((permissionid) =>
                    FetchApi(`/api/Permission/${permissionid}/${status}`, {
                        method: "PATCH",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify(
                            expiryDate? expiryDate : null
                        ),
                    }
                    ))
            );
            onComplete();
        } catch (error) {
            setErrorMsg(`${error}`)
        }
    }


    return (
        <div className="flex justify-between items-center px-5 py-4 border-b border-gray-200">

            <div className="flex flex-col gap-2">

                <p className="text-sm text-gray-900 "> {company} </p>
                <p className="text-xs text-gray-500"> {date} </p>

                <div className="flex gap-3 flex-wrap">
                    {checkedFields.map((field) => (
                        <label key={field.label}
                            className="flex items-center gap-2 border border-gray-300  rounded-md px-3 py-1 text-sm text-gray-700 cursor-pointer">
                            <input
                                type="checkbox"
                                checked={field.checked}
                                onChange={() => toggleField(field.label)}
                                className="cursor-pointer"

                            />
                            {field.label}
                        </label>
                    ))}
                    {checkedUnlistedFields.map((field) => (
                        <label key={field.label}
                            className="flex items-center gap-2 border border-gray-300  rounded-md px-3 py-1 text-sm text-gray-700 cursor-pointer">
                            <input
                                type="checkbox"
                                checked={field.checked}
                                onChange={() => toggleField(field.label)}
                                className="cursor-pointer"

                            /> 
                            {field.label}
                            <div className="w-5 h-5 rounded-full bg-yellow-400 flex items-center justify-center">
                                <span className="text-white font-bold text-sm"> ! </span>
                            </div>
                        </label>
                    ))}
                </div>
                <div className="flex items-center gap-1 text-xs text-gray-500 ">
                    <span>{reason}</span>
                </div>
            </div>

            <div className="flex gap-2">
                <button className="bg-green-500 hover:bg-green-600 text-white rounded-md px-4 py-2 text-base disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer transition-colors"
                    disabled={checkedFields.filter(f => f.checked).length === 0 && checkedUnlistedFields.filter(f => f.checked).length === 0}
                    onClick={() => {
                        if (checkedUnlistedFields.filter(f => f.checked).length > 0 ) {
                            setShowAddInfoModal(true)
                        } else {
                            setShowModal(true);
                            setShowExpiry(true);
                        }
                        setPendingAction("approve");
                        
                    }}
                >✔</button>
                <button className="bg-red-500 hover:bg-red-600 text-white rounded-md px-4 py-2 text-base disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer transition-colors"
                    disabled={checkedFields.filter(f => f.checked).length === 0 && checkedUnlistedFields.filter(f => f.checked).length === 0}
                    onClick={() => {
                        setShowModal(true);
                        setPendingAction("reject")
                        setShowExpiry(false);
                    }}

                >✖</button>
                {errorMsg && <p className="text-sm text-red-500">{errorMsg}</p>}
                
                {showAddInfoModal && (
                    <AddPersonalInfoModal
                        company={company}
                        unlistedInfoDesc= {checkedUnlistedFields
                                .filter((f) => f.checked)
                                .map((f) => f.label)}
                        onCancel={() => {
                            setShowAddInfoModal(false);
                        }}
                        
                        onNext={() => {
                            setShowAddInfoModal(false);
                            setShowModal(true);
                        }}
                        
                    />
                )}

                {showModal && (
                    <ConfirmModal
                        company={company}
                        status={pendingAction}
                        selectedFields={combined
                            .filter((f) => f.checked)
                            .map((f) => f.label)}
                        onConfirm={() => {
                            if (pendingAction) changePermission(pendingAction as "approve" | "reject");
                            setShowModal(false);
                        }}
                        onCancel={() => {
                            setShowModal(false);
                            setPendingAction("");
                        }}
                        showExpiry={showExpiry}
                        choseExpiry={onExpiry}
                    />
                )}

                
            </div>

        </div>
    )
}