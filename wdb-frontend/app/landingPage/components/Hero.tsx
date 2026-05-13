
import Image from "next/image";
import Link from "next/link";
import CompanyLogo from "./../../assets/CompanyLogo";
import heroBackground from "./../../assets/heroBackground.png";
import blockchainLogo from "./../../assets/blockchainLogo.png";
import firstStepSolutions from "./../../assets/firstStepSolutions.png";

export default function Hero() {
    return (
        <div className="relative h-[200vh] flex flex-col bg-[#374151] shrink-0">

            <div className="absolute inset-0" />
            <Image
                src={heroBackground}
                alt=""
                fill
                className="object-cover object-center opacity-30"
                priority
            />

            {/* First screen — nav + headline + logos */}
            <div className="relative z-10 flex flex-col h-screen snap-start shrink-0">

                <nav className="flex items-center px-10 py-7">
                    <CompanyLogo height={30} />
                </nav>

                {/* Centered headline */}
                <div className="flex flex-1 items-center justify-center px-6">
                    <div className="flex flex-col items-center text-center gap-6 max-w-2xl animate-fadeUp">
                        <h1 className="font-extrabold text-white leading-[1.05] tracking-[-0.03em] text-[clamp(44px,7vw,48px)]">
                            Your Data,
                            <br />
                            Your Sovereignty
                        </h1>
                        <p className="text-[17px] font-light leading-relaxed text-gray-200">
                            Gain full control of your professional information with{" "}
                            <span className="font-bold text-xl">workPass</span>. <br />
                            Enhanced with blockchain technology.
                        </p>
                        <div className="flex items-center gap-3 mt-2">
                            <Link href="/login">
                                <button className="cursor-pointer bg-gray-200 text-gray-700 font-bold text-sm tracking-wide px-7 py-3.5 rounded-2xl hover:bg-yellow-200 transition-opacity">
                                    Get Started
                                </button>
                            </Link>
                        </div>
                    </div>
                </div>

                {/* Logos pinned to bottom of first screen */}
                <div className="flex justify-center items-center gap-8 pb-10">
                    <Image src={firstStepSolutions} alt="" width={0} height={0} style={{ height: '40px', width: 'auto' }} priority />
                    <Image src={blockchainLogo} alt="" width={0} height={0} style={{ height: '60px', width: 'auto' }} priority />
                </div>

            </div>

            {/* Problem statement */}
            <div className="relative z-10 flex flex-col items-center justify-center px-10 h-screen snap-start shrink-0">

                <div className="max-w-2xl leading-relaxed font-thin text-gray-200 text-sm">
                    <p className="mb-3 ">

                        Workers today have little control over their own professional data.
                    </p>
                    <br />
                    <p className="mb-3">
                        Employers
                        and third-party systems collect, store, and manage personal information
                        including certifications, training records, health details and emergency
                        contacts, often without clear transparency to the worker.
                    </p>
                    <br />
                    <p className="mb-3">Workers rarely know what data is held about them or where it lives.</p>
                    <br />
                    <p className="mb-3">Access is controlled by employers, not the individuals the data belongs to.</p>
                    <br />
                    <p className="mb-3">The same information gets re-entered across every new employer or system.</p>
                    <br />
                    <p className="mb-3">Sensitive data is duplicated, fragmented, and difficult to audit.</p>
                    <br />
                    <p className=" text-gray-300 font-light">
                        WorkPass gives workers a transparent, permission-based system where every
                        access event is recorded, auditable, and in their own hands.

                    </p>
                    <br />
                    <p className=" text-gray-300 font-light">
                        Blockchain technology offers a potential solution by enabling transparent, permission-based, and auditable data sharing without requiring a single central owner.

                    </p>
                </div>
            </div>

        </div>
    );
}