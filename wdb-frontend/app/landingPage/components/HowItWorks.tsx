
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
    return (
        <section className="flex flex-col items-center gap-10">

            <h2 className="text-[44px] font-bold text-slate-600">How <span className="font-normal">workPass</span> Works</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 w-full max-w-6xl">
                {steps.map((step) => (
                    <div key={step.number} className="flex flex-col gap-4">

                        <span className="text-3xl font-extrabold text-gray-600 tracking-tight text-center">
                            {step.number}
                        </span>

                        <div className="flex flex-col gap-4 rounded-2xl border border-gray-200 bg-white p-6 shadow-sm h-full">

                            <div className="flex items-center justify-center h-32">
                                <Image
                                    src={step.icon}
                                    alt={step.title}
                                    height={100}
                                    className="object-contain"
                                />
                            </div>

                            <h3 className="text-2xl font-extrabold text-gray-600 tracking-wide text-center">
                                {step.title}
                            </h3>

                            <p className="text-sm text-gray-500 leading-relaxed">
                                {step.description}
                            </p>

                        </div>
                    </div>
                ))}
            </div>

            <div className="w-full max-w-6xl rounded-2xl border border-gray-200 bg-white py-5 px-8 text-center shadow-sm">
                <p className="text-sm text-gray-500">
                    Every transactions are recorded in <span className="font-extrabold text-2xl text-gray-600 ">BLOCKCHAIN</span>
                </p>
            </div>

        </section>
    );
}