
'use client'

import { useState } from "react";

const faqs = [
    {
        question: "How does blockchain works in this application?",
        answer:
            "Every transaction creates a block. Each block is locked to the block before it, forming a chain. To change any record, you would need to break every single lock in the chain, which is virtually impossible. That is what makes the data permanent and tamper-proof.",
    },
    {
        question: "How does this application provide more data control to user?",
        answer:
            "Every employer require to send an access request before viewing any of your information. You review the request and choose to approve or reject it. If approved, you can set an expiry date so access ends automatically, or revoke it manually at any time. Every one of these actions is permanently recorded on the blockchain, giving you full visibility and control over your personal data at every step.",
    },
];

export default function FAQ() {
    const [openIndex, setOpenIndex] = useState<number | null>(0);

    const toggle = (index: number) => {
        setOpenIndex(openIndex === index ? null : index);
    };

    return (
        <section className=" px-10 flex flex-col items-center gap-4 w-full">

            {/* Section heading */}
            <h2 className="text-3xl font-extrabold text-gray-800 mb-6">
                Frequently Asked Questions
            </h2>

            <div className="flex flex-col gap-4 w-full max-w-4xl">
                {faqs.map((faq, index) => (
                    <div
                        key={index}
                        className="rounded-2xl border border-gray-300 bg-white overflow-hidden"
                    >
                        {/* Question row */}
                        <button
                            onClick={() => toggle(index)}
                            className="w-full flex items-center justify-between px-8 py-5 text-left"
                        >
                            <span className="font-bold text-gray-700 text-[15px]">
                                {faq.question}
                            </span>
                        </button>

                        {/* Answer row*/}
                        {openIndex === index && (
                            <div className="px-8 pb-6">
                                <p className="text-gray-500 text-sm leading-relaxed text-left">
                                    {faq.answer}
                                </p>
                            </div>
                        )}
                    </div>
                ))}
            </div>

        </section>
    );
}