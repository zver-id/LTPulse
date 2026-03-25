import { Table, Tag } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useGetTicketsQuery } from '../../storage/services/tickets-api.ts'

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
    type: string
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

    const date = getCurrentDate();

    const { data: tickets } = useGetTicketsQuery({teamId, date, ticketType});
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
        }
    ];

    return (
        <>
            <Table dataSource={tickets} columns={columns} />
        </>
    )
}

export default TicketScreen;
