import { Table, Tag } from 'antd';
import type { ColumnsType } from 'antd/es/table';

interface ITicket {
    key: number;
    name: string;
    state: string;
    employee: string;
    timeInWork: number;
    hyperlink: string;
}

function TicketScreen() {
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

    const dataSource: ITicket[] = [
        { key: 1, name: 'Тикет 1 Какой то гигааааааааааааааааааааааааааааааантский текст для примера, потому что тут будет очень много всякого',
            state: 'At work', employee: 'Человек 1', timeInWork: 3.2, hyperlink: 'https://google.com' },
        { key: 2, name: 'Тикет 2', state: 'Await', employee: 'Человек 2', timeInWork: 12, hyperlink: 'https://google.com'  },
    ];

    return (
        <>
            <Table dataSource={dataSource} columns={columns} />
        </>
    )
}

export default TicketScreen;