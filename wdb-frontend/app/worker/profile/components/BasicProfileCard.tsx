import { useEffect, useState } from "react"
import { WorkerInfoItem } from "../type"
import TextInput from "@/component/ui/TextInput"
import Dropdown from "@/component/ui/Dropdown"
import { DisplayField } from '@/component/ui/DisplayField'

interface BasicProfileCardProps {
    data: WorkerInfoItem[]
    onSave: (desc: string, value: string, category: string) => Promise<void>
}

export default function BasicProfileCard({ data, onSave }: BasicProfileCardProps) {

    const [isEditing, setIsEdit] = useState(false)
    const [editFirstName, setEditFirstName] = useState('')
    const [editLastName, setEditLastName] = useState('')
    const [editPhone, setEditPhone] = useState('')
    const [editEmail, setEditEmail] = useState('')
    const [editCountry, setEditCountry] = useState('')
    const [editCity, setEditCity] = useState('')
    const [editStreet, setEditStreet] = useState('')
    const [editPostcode, setEditPostcode] = useState('')
    const [editGendar, setEditGendar] = useState('')

    const genderOptions = [
        { label: 'Male', value: 'male' },
        { label: 'Female', value: 'female' },
        { label: 'Non-binary', value: 'non-binary' },
        { label: 'Prefer not to say', value: 'prefer_not_to_say' },
    ]

    useEffect(() => {
        setEditFirstName(data.find(item => item.desc == 'firstname')?.value ?? '')
        setEditLastName(data.find(item => item.desc == 'lastname')?.value ?? '')
        setEditPhone(data.find(item => item.desc == 'phonenumber')?.value ?? '')
        setEditEmail(data.find(item => item.desc == 'email')?.value ?? '')
        setEditCountry(data.find(item => item.desc == 'country')?.value ?? '')
        setEditCity(data.find(item => item.desc == 'city')?.value ?? '')
        setEditStreet(data.find(item => item.desc == 'street')?.value ?? '')
        setEditPostcode(data.find(item => item.desc == 'postcode')?.value ?? '')
        setEditGendar(data.find(item => item.desc == 'gender')?.value ?? '')
    }, [data])

    const handleDone = async () => {
        if (!editFirstName.trim()) { alert('First name cannot be empty'); return }
        if (!editLastName.trim()) { alert('Last name cannot be empty'); return }
        if (!editEmail.trim()) { alert('Email cannot be empty'); return }
        if (!editPhone.trim()) { alert('Phone number cannot be empty'); return }
        if (!editPostcode.trim()) { alert('Post code cannot be empty'); return }
        if (!editGendar.trim()) { alert('Gender cannot be empty'); return }
        if (!editCountry.trim()) { alert('Country cannot be empty'); return }
        if (!editCity.trim()) { alert('City cannot be empty'); return }
        if (!editStreet.trim()) { alert('Street cannot be empty'); return }

        await onSave('firstname', editFirstName, 'PersonaInformation')
        await onSave('lastname', editLastName, 'PersonaInformation')
        await onSave('phonenumber', editPhone, 'PersonaInformation')
        await onSave('email', editEmail, 'PersonaInformation')
        await onSave('country', editCountry, 'PersonaInformation')
        await onSave('city', editCity, 'PersonaInformation')
        await onSave('street', editStreet, 'PersonaInformation')
        await onSave('postcode', editPostcode, 'PersonaInformation')
        await onSave('gender', editGendar, 'PersonaInformation')

        setIsEdit(false)
    }

    return (
        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
            <div className="flex items-center justify-between mb-6">
                <h2 className="text-lg font-semibold text-slate-900">Basic Information</h2>
                <button
                    onClick={isEditing ? handleDone : () => setIsEdit(true)}
                    className="text-sm font-semibold text-blue-600 hover:text-blue-700"
                >
                    {isEditing ? 'Done' : 'Edit'}
                </button>
            </div>

            {isEditing ? (
                <div className="flex flex-col gap-4">
                    <div className="grid grid-cols-2 gap-4">
                        <TextInput label="First Name" value={editFirstName} onChange={(v) => setEditFirstName(v)} />
                        <TextInput label="Last Name" value={editLastName} onChange={(v) => setEditLastName(v)} />
                        <TextInput label="Phone" value={editPhone} onChange={(v) => setEditPhone(v)} />
                        <TextInput label="Email" value={editEmail} onChange={(v) => setEditEmail(v)} />
                    </div>

                    <p className="text-sm font-medium text-slate-500 mt-2">Address</p>
                    <div className="grid grid-cols-2 gap-4 pl-4 border-l-2 border-slate-200">
                        <TextInput label="Country" value={editCountry} onChange={(v) => setEditCountry(v)} />
                        <TextInput label="City" value={editCity} onChange={(v) => setEditCity(v)} />
                        <TextInput label="Street" value={editStreet} onChange={(v) => setEditStreet(v)} />
                        <TextInput label="PostCode" value={editPostcode} onChange={(v) => setEditPostcode(v)} />
                    </div>

                    <Dropdown label="Gender" value={editGendar} onChange={(v) => setEditGendar(v)} options={genderOptions} />
                </div>
            ) : (
                <div className="grid gap-4 md:grid-cols-2">
                    <div>
                        <p className="text-sm text-slate-500">First Name</p>
                        <p className="mt-1 font-medium text-slate-900">{editFirstName || '-'}</p>
                    </div>
                    <div>
                        <p className="text-sm text-slate-500">Last Name</p>
                        <p className="mt-1 font-medium text-slate-900">{editLastName || '-'}</p>
                    </div>
                    <div>
                        <p className="text-sm text-slate-500">Phone</p>
                        <p className="mt-1 font-medium text-slate-900">{editPhone || '-'}</p>
                    </div>
                    <div>
                        <p className="text-sm text-slate-500">Email</p>
                        <p className="mt-1 font-medium text-slate-900">{editEmail || '-'}</p>
                    </div>
                    <div>
                        <p className="text-sm text-slate-500">Country</p>
                        <p className="mt-1 font-medium text-slate-900">{editCountry || '-'}</p>
                    </div>
                    <div>
                        <p className="text-sm text-slate-500">City</p>
                        <p className="mt-1 font-medium text-slate-900">{editCity || '-'}</p>
                    </div>
                    <div>
                        <p className="text-sm text-slate-500">Street</p>
                        <p className="mt-1 font-medium text-slate-900">{editStreet || '-'}</p>
                    </div>
                    <div>
                        <p className="text-sm text-slate-500">PostCode</p>
                        <p className="mt-1 font-medium text-slate-900">{editPostcode || '-'}</p>
                    </div>
                    <div>
                        <p className="text-sm text-slate-500">Gender</p>
                        <p className="mt-1 font-medium text-slate-900">{editGendar || '-'}</p>
                    </div>
                </div>
            )}
        </section>
    )
}

