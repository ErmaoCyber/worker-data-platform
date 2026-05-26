"use client";

import { useState, useEffect } from 'react'
import { FetchApi } from '../../lib/api';
import { addWorkerProfile } from '@/lib/api/workerApi'
import { useAuth } from '@/context/AuthContext'

interface AddInfoProps {
    company: string;
    unlistedInfoDesc: string[];
    onNext: () => void;
    onCancel: () => void;
}

interface WorkerInfoItem {
    id?: string
    workerId: string
    desc: string
    value: string
}

export default function AddPersonalInfoModal({ company, unlistedInfoDesc, onNext, onCancel}: AddInfoProps) {
    // const [token, setToken] = useState('')
    // centralised auth state, so the modal does not duplicate the localStorage read logic and stays consistent with the rest of the app
    const { token } = useAuth()
    // walk through each unlisted item one at a time so multi-item requests can all be filled in
    const [currentIndex, setCurrentIndex] = useState(0)
    const [value, setValue] = useState('')
    const [category, setCategory] = useState('Personal')
    const [error, setError] = useState('')
    // desc is derived from the current index, not its own state, so it always tracks the active item
    const desc = unlistedInfoDesc[currentIndex] ?? ''
    // const [allDate, setAllData] = useState<WorkerInfoItem[]>([])

    // useEffect(() => {
    //         const storedToken = localStorage.getItem('accessToken');
    //         if (storedToken != null) {
    //             setToken(storedToken);
    //         }
    //     }, []);

    // clear the form between items so the user always starts fresh after advancing
    useEffect(() => {
        setValue('')
        setCategory('Personal')
        setError('')
    }, [currentIndex]);

    async function addNewPersonalInfo() {
        // useAuth().token can be null before login is restored; skip the doomed request
        if (!token) return
        try {
            await addWorkerProfile(token, desc, value, category)
        } catch (error) {
            console.error('Failed to add worker profile:', error)
        }

    }
    return (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
            <div className="bg-white  rounded-2xl shadow-lg p-6 w-full max-w-sm mx-4">

                <h2 className="text-base font-semibold text-gray-900  mb-4">
                    <span className="text-gray-500">{company} </span> is requesting for:
                </h2>

                <ul className="mb-6 flex flex-col gap-2">
                    {unlistedInfoDesc.map((field, idx) => (
                        <li
                            key={field}
                            className={
                                idx === currentIndex
                                    ? "text-sm text-gray-900 border border-red-400 bg-red-50 rounded-md px-3 py-1.5"
                                    : idx < currentIndex
                                        ? "text-sm text-gray-400 border border-gray-200 rounded-md px-3 py-1.5 line-through"
                                        : "text-sm text-gray-700 border border-gray-200 rounded-md px-3 py-1.5"
                            }
                        >
                            {field}
                        </li>
                    ))}
                </ul>

                <h2 className="text-base font-semibold text-gray-900  mb-1">
                    Add New Personal Info{unlistedInfoDesc.length > 1 ? ` (${currentIndex + 1} of ${unlistedInfoDesc.length})` : ''}
                </h2>
                <label className="block text-sm text-gray-400 mb-0.5">Description</label>
                <input
                value={desc}
                readOnly
                className="w-full py-[11px] px-[14px] border border-[#D9D9D9] rounded-lg text-[0.9rem] text-gray-500 bg-gray-50 outline-none mb-3 cursor-not-allowed"
                />
                <label className="block text-sm text-gray-400 mb-0.5">Value</label>
                <input
                placeholder="e.g. Covid-19 Vaccine 2021"
                value={value}
                onChange={(e) => setValue(e.target.value)}
                required
                className="w-full py-[11px] px-[14px] border border-[#D9D9D9] rounded-lg text-[0.9rem] text-black bg-white outline-none mb-3"
                />
                <label className="block text-sm text-gray-400 mb-0.5">Category</label>
                <select
                    value={category}
                    onChange={(e) => setCategory(e.target.value)}
                    className="w-full py-[11px] px-[14px] border border-[#D9D9D9] rounded-lg text-[0.9rem] text-black bg-white outline-none mb-4">
                    <option>Personal</option>
                    <option>Medical</option>
                    <option>Career</option>
                    <option>Other</option>
                </select>

                {error && <p className="flex text-sm text-red-500 items-center justify-center">{error}</p>}
        
                <div className="flex gap-3 justify-end mt-6">
                    <button
                        onClick={onCancel}
                        className="px-4 py-2 text-sm rounded-lg border border-gray-300  text-gray-700  hover:bg-gray-100 transition-colors cursor-pointer"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={async () => {
                            if (!desc || !value || !category) {
                                setError('Fill in all details')
                            } else {
                                await addNewPersonalInfo()
                                // advance to next unlisted item; only when all are filled do we hand off to the next stage
                                if (currentIndex < unlistedInfoDesc.length - 1) {
                                    setCurrentIndex(currentIndex + 1)
                                } else {
                                    onNext()
                                }
                            }
                        }}
                        className="px-4 py-2 text-sm rounded-lg bg-red-500 hover:bg-red-600 text-white transition-colors cursor-pointer"
                    >
                        {currentIndex < unlistedInfoDesc.length - 1 ? 'Next' : 'Add New Info'}
                    </button>
                </div>

            </div>
        </div>
    );
}