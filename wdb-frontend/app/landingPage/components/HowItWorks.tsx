'use client'

import { useState } from "react";
import Image from "next/image";
import requestIcon from "../../assets/request.png";
import approveIcon from "../../assets/approve.png";
import viewIcon from "../../assets/view.png";
import revokeIcon from "../../assets/revoke.png";

const steps = [
    {
        number: "01",
        icon: requestIcon,
        title: "REQUEST",
        description: "Employer submit request to access certain information from employee",
    },
    {
        number: "02",
        icon: approveIcon,
        title: "APPROVE",
        description: "Employee approve on selected information to be shared",
    },
    {
        number: "03",
        icon: viewIcon,
        title: "VIEW",
        description: "Employer gain access to view on approved information",
    },
    {
        number: "04",
        icon: revokeIcon,
        title: "REVOKE",
        description: "Employee able to set expiry date on the access upon approval or revoke access at anytime",
    },
];

export default function HowItWorks() {
    const [hoveredIndex, setHoveredIndex] = useState<number | null>(null);

    return (
        <section className={`h-screen py-24 px-10  flex flex-col items-center gap-10 transition-colors duration-300 ${hoveredIndex !== null ? "bg-gray-800" : "bg-white"
            }`}>

            <h2 className={`text-[44px] font-bold transition-colors duration-300 ${hoveredIndex !== null ? "text-gray-200" : "text-slate-600"
                }`}>
                How <span className="font-normal">workPass</span> Works
            </h2>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 w-full max-w-6xl">
                {steps.map((step, index) => {
                    const isHovered = hoveredIndex === index;
                    const isDimmed = hoveredIndex !== null && !isHovered;

                    return (
                        <div
                            key={step.number}
                            className="flex flex-col gap-4"
                            onMouseEnter={() => setHoveredIndex(index)}
                            onMouseLeave={() => setHoveredIndex(null)}
                        >
                            <span className={`text-3xl font-extrabold tracking-tight text-center transition-colors duration-300 ${isHovered ? "text-gray-300" : "text-gray-600"
                                }`}>
                                {step.number}
                            </span>

                            <div className={`flex flex-col gap-4 rounded-2xl border p-6 shadow-sm h-full transition-all duration-300 ${isDimmed
                                ? "bg-gray-800 border-gray-700"
                                : "bg-white border-gray-200"
                                }`}>

                                <div className="flex items-center justify-center h-32">
                                    <Image
                                        src={step.icon}
                                        alt={step.title}
                                        height={100}
                                        className={`object-contain transition-opacity duration-300 ${isDimmed ? "opacity-30" : "opacity-100"
                                            }`}
                                    />
                                </div>

                                <h3 className={`text-2xl font-extrabold tracking-wide text-center transition-colors duration-300 ${isDimmed ? "text-gray-500" : "text-gray-600"
                                    }`}>
                                    {step.title}
                                </h3>

                                <p className={`text-sm leading-relaxed transition-colors duration-300 ${isDimmed ? "text-gray-600" : "text-gray-500"
                                    }`}>
                                    {step.description}
                                </p>

                            </div>
                        </div>
                    );
                })}
            </div>

            {/* RECORD */}
            <div className={`w-full max-w-6xl rounded-2xl border px-8 py-6 shadow-sm transition-all duration-300 bg-white border-gray-200`}>

                <div className="text-center mb-6">
                    <h3 className={`text-2xl font-extrabold tracking-wide transition-colors duration-300 text-gray-700`}>
                        RECORD
                    </h3>
                    <p className={`text-sm mt-1 transition-colors duration-300 text-gray-500`}>
                        Every transactions are recorded in blockchain
                    </p>
                </div>

                <div className="relative flex w-fit mx-auto gap-8">

                    {/* Link bar */}
                    <div className={`absolute top-1/2 -translate-y-1/2 left-0 right-0 h-4 rounded-full transition-colors duration-300 bg-gray-600`} />

                    {/* Boxes */}
                    {steps.map((step, index) => {
                        const isHovered = hoveredIndex === index;
                        const isDimmed = hoveredIndex !== null && !isHovered;

                        return (
                            <div
                                key={step.number}
                                className={`relative z-10 p-5 aspect-square max-w-[100px] w-full flex items-center justify-center rounded-xl border transition-all duration-300 ${isDimmed
                                    ? "bg-white border-gray-200 "
                                    : "bg-gray-800 border-gray-700"
                                    }`}
                            >
                                <Image
                                    src={step.icon}
                                    alt={step.title}
                                    height={50}
                                    className={`object-contain transition-opacity duration-300 ${isDimmed ? "opacity-50" : "opacity-100"
                                        }`}
                                />
                            </div>
                        );
                    })}
                </div>

            </div>

        </section>
    );
}