
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
    const [openIndex, setOpenIndex] = useState<number | null>(null);

    const toggle = (index: number) => {
        setOpenIndex(openIndex === index ? null : index);
    };

    return (
        <section className="px-10 flex flex-col items-center gap-4 w-full">

            <h2 className="text-[44px] font-extrabold text-gray-700 mb-6 text-center">
                Frequently <br /> Asked Questions
            </h2>

            <div className="flex flex-col gap-4 w-full max-w-4xl">
                {faqs.map((faq, index) => {
                    const isClicked = openIndex === index;
                    return (
                        <div
                            key={index}
                            className={`group w-full rounded-2xl border overflow-hidden transition-colors duration-300 ${isClicked
                                ? "bg-gray-800 border-gray-700"
                                : "bg-white border-gray-300 hover:bg-gray-800 hover:border-gray-700"
                                }`}
                        >
                            <button
                                onClick={() => toggle(index)}
                                className="w-full flex items-center justify-between px-8 py-5 text-left"
                            >
                                <span className={`font-bold text-[15px] transition-colors duration-300 ${isClicked
                                    ? "text-white"
                                    : "text-gray-700 group-hover:text-white"
                                    }`}>
                                    {faq.question}
                                </span>
                                <span className={`text-xl ml-4 shrink-0 transition-colors duration-300 ${isClicked
                                    ? "text-white"
                                    : "text-gray-400 group-hover:text-white"
                                    }`}>
                                    {isClicked ? "−" : "+"}
                                </span>
                            </button>

                            {/* Shows on click or on hover*/}
                            <div className={`px-8 transition-all duration-300 ${isClicked
                                ? "max-h-96 pb-6 opacity-100"
                                : "max-h-0 pb-0 opacity-0 group-hover:max-h-96 group-hover:pb-6 group-hover:opacity-100"
                                }`}>
                                <p className="text-gray-300 text-sm leading-relaxed text-left">
                                    {faq.answer}
                                </p>
                            </div>

                        </div>
                    );
                })}
            </div>

        </section>
    );
}