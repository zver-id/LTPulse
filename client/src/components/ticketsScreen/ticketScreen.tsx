import {Button, Card, DatePicker, Space, Table, Tag, Typography} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useGetTicketsQuery } from '../../storage/services/tickets-api.ts'
import {useState} from "react";
import { Dayjs } from 'dayjs';
import { EditOutlined } from '@ant-design/icons';
import EditTicketModal from '../editTicketModal/editTicketModal.tsx'


export interface ITicket {
    key: number;
    name: string;
    state: string;
    employee: string;
    timeInWork: number;
    hyperlink: string;
    timeStampedOnDay: number;
    incomingDate: string;
    priority: string;
    organization: string;
    type: string;
    comment: string;
}

export interface ITicketScreenProps {
    teamId: number;
    ticketType: string;
}

function TicketScreen({teamId, ticketType} :ITicketScreenProps) {

    const getCurrentDate = (): string => {
        const now = new Date();
        const day = String(now.getDate()).padStart(2, '0');
        const month = String(now.getMonth() + 1).padStart(2, '0');
        const year = now.getFullYear();

        return `${year}-${month}-${day}`;
    };

    const [dateJs, setDateJs] = useState<Dayjs | null>(null);
    const date = dateJs?.format('YYYY-MM-DD') || getCurrentDate();

    const [modalOpen, setModalOpen] = useState(false);
    const [selectedTicket, setSelectedTicket] = useState<ITicket | null>(null);
    const [saving, setSaving] = useState(false);

    const { data: tickets } = useGetTicketsQuery({teamId, date, ticketType});

    const handleEdit = (record: ITicket) => {
        setSelectedTicket(record);
        setModalOpen(true);
    };

    const handleCloseModal = () => {
        setModalOpen(false);
        setSelectedTicket(null);
        setSaving(false);
    };

    const columns: ColumnsType<ITicket> = [
        {
            title: 'Номер обращения',
            dataIndex: 'key',
            key: 'key',
            sorter: (a, b) => a.key - b.key,
            render: (text: string, record: ITicket) => {
                return <a href={record.hyperlink} target="_blank">
                    {text}
                </a>;
            }
        },
        {
            title: 'Описание',
            dataIndex: 'name',
            key: 'name'
        },
        {
            title: 'Состояние обращения',
            dataIndex: 'state',
            key: 'state',
            sorter: (a, b) => a.state.localeCompare(b.state)
        },
        {
            title: 'Ответственный',
            dataIndex: 'employee',
            key: 'employee',
            sorter: (a, b) => a.employee.localeCompare(b.employee)
        },
        {
            title: 'Время в работе',
            dataIndex: 'timeInWork',
            key: 'timeInWork',
            sorter: (a, b) => a.timeInWork - b.timeInWork,
            defaultSortOrder: 'descend',
            render: (timeInWork: number) =>{
                let color: string = ''
                if (timeInWork <= 8){
                    color = 'green'
                }
                else if (8 < timeInWork && timeInWork <= 16){
                    color = '#f4e5c2' //Песочный цвет
                }
                else if (16 < timeInWork && timeInWork <= 24){
                    color = 'yellow'
                }
                else {
                    color = 'red'
                }
                return <Tag color={color}>{timeInWork}</Tag>;
            }
        },
        {
            title: '',
            key: 'action',
            fixed: 'right',
            width: 120,
            render: (_, record) => (
                <Space size="small">
                    <Button
                        type="link"
                        icon={<EditOutlined/>}
                        onClick={() => handleEdit(record)}
                    >
                        Edit
                    </Button>
                </Space>
            ),
        }
    ];

    return (
        <div>
            <DatePicker
                value={dateJs}
                onChange={(newDate) => setDateJs(newDate)}
                format="DD-MM-YYYY"
                style={{ width: 200 }}
            />
            {(!tickets || tickets.length === 0) &&
                <Card>
                    <Typography>Обращений за эту дату нет. Похоже расчет не производился.</Typography>
                </Card>
            }
            {
                tickets && tickets.length > 0 &&
                <>
                    <Table dataSource={tickets} columns={columns} />
                    <EditTicketModal isOpen={modalOpen}
                                     ticket={selectedTicket}
                                     loading={saving}
                                     onClose={handleCloseModal}
                                     onSuccess={handleEdit}/>
                </>
            }
        </div>
    )
}

export default TicketScreen;
