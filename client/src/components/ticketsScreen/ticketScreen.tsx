import {Button, Card, DatePicker, Space, Table, Tag, Typography} from 'antd';
import type {ColumnsType} from 'antd/es/table';
import {useGetTicketsQuery} from '../../storage/services/tickets-api.ts'
import {useState} from "react";
import dayjs, {Dayjs} from 'dayjs';
import {EditOutlined} from '@ant-design/icons';
import EditItemModal from '../editItemModal/editItemModal.tsx'


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

function TicketScreen({teamId, ticketType}: ITicketScreenProps) {

  const [dateJs, setDateJs] = useState<Dayjs | null>(dayjs());
  const date = dateJs?.format('YYYY-MM-DD') ||
    new Date().toISOString().split('T')[0];

  const [modalOpen, setModalOpen] = useState(false);
  const [selectedTicket, setSelectedTicket] = useState<ITicket | null>(null);
  const [saving, setSaving] = useState(false);

  const {data: tickets} = useGetTicketsQuery({teamId, date, ticketType});

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
      filters: Array.from(new Set(tickets?.map((item: ITicket):string => item.state)))
        .sort((a:string, b:string) => a.localeCompare(b))
        .map(state => ({text: state, value: state})),
      onFilter: (value, record) => record.state === value,
      sorter: (a, b) => a.state.localeCompare(b.state)
    },
    {
      title: 'Ответственный',
      dataIndex: 'employee',
      key: 'employee',
      sorter: (a, b) => a.employee.localeCompare(b.employee)
    },
    {
      title: 'Организация',
      dataIndex: 'organization',
      key: 'organization',
      sorter: (a, b) => a.employee.localeCompare(b.employee)
    },
    {
      title: 'Время в работе',
      dataIndex: 'timeInWork',
      key: 'timeInWork',
      sorter: (a, b) => a.timeInWork - b.timeInWork,
      defaultSortOrder: 'descend',
      render: (timeInWork: number) => {
        let color: string = ''
        if (timeInWork <= 8) {
          color = 'green'
        } else if (8 < timeInWork && timeInWork <= 16) {
          color = '#c2a56d' //Песочный цвет
        } else if (16 < timeInWork && timeInWork <= 24) {
          color = 'yellow'
        } else {
          color = 'red'
        }
        return <Tag color={color}>{timeInWork.toFixed(2)}</Tag>;
      }
    },
    {
      title: 'Комментарий',
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
        style={{width: 200}}
      />
      {(!tickets || tickets.length === 0) &&
        <Card>
          <Typography>Здесь ничего нет.</Typography>
            <img
              src="/gif/where.gif"
              width={400}
              height={300}
            />
        </Card>
      }
      {
        tickets && tickets.length > 0 &&
        <>
          <Table dataSource={tickets} columns={columns}/>
          <EditItemModal isOpen={modalOpen}
                         item={selectedTicket}
                         loading={saving}
                         onClose={handleCloseModal}
                         onSuccess={handleEdit}/>
        </>
      }
    </div>
  )
}

export default TicketScreen;
