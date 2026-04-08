import {useState} from "react";
import dayjs, {Dayjs} from "dayjs";
import type {ColumnsType} from "antd/es/table";
import {Button, Card, DatePicker, Space, Table, Tag, Typography} from "antd";
import {EditOutlined} from "@ant-design/icons";
import EditTicketModal from "../editTicketModal/editTicketModal.tsx";
import {useGetGradesQuery} from "../../storage/services/grade-api.ts";
import type {ITicket} from "../ticketsScreen/ticketScreen.tsx";

export interface IGrade {
  key: number,
  text: string,
  score: number,
  date: string,
  isResearched: boolean,
  ticketHyperlink: string,
  employee: string
}

export interface IGradeScreenProps {
  teamId: number;

}

function GradeScreen({teamId}: IGradeScreenProps) {

  const [dateJs, setDateJs] = useState<Dayjs | null>(dayjs());
  const date = dateJs?.format('YYYY-MM-DD') ||
    new Date().toISOString().split('T')[0];

  const [onlyUnresearched, setOnlyUnresearched] = useState(false);
  const [onlyNegative, setOnlyNegative] = useState(false);

  const [modalOpen, setModalOpen] = useState(false);
  const [selectedGrade, setSelectedGrade] = useState<IGrade | null>(null);
  const [saving, setSaving] = useState(false);

  const {data: grades} = useGetGradesQuery({teamId, date, onlyUnresearched, onlyNegative});

  const handleEdit = (record: ITicket) => {
    setSelectedTicket(record);
    setModalOpen(true);
  };

  const handleCloseModal = () => {
    setModalOpen(false);
    setSelectedTicket(null);
    setSaving(false);
  };

  const columns: ColumnsType<IGrade> = [
    {
      title: 'Номер обращения',
      dataIndex: 'key',
      key: 'key',
      sorter: (a, b) => a.key - b.key,
      render: (text: string, record: IGrade) => {
        return <a href={record.ticketHyperlink} target="_blank">
          {text}
        </a>;
      }
    },
    {
      title: 'Текст оценки',
      dataIndex: 'text',
      key: 'text'
    },
    {
      title: 'Оценка',
      dataIndex: 'score',
      key: 'score',
      filters: Array.from(new Set(grades?.map((item) => item.score)))
        .sort((a, b) => a - b )
        .map(score => ({text: score === 2 ? "Плохо" : "Хорошо", value: score})),
      onFilter: (value, record) => record.score === value,
      render: (record: IGrade) => {
        return <span>{record.score === 2 ? "Плохо" : "Хорошо"}</span>;
      }
    },
    {
      title: 'Ответственный',
      dataIndex: 'employee',
      key: 'employee',
      sorter: (a, b) => a.employee.localeCompare(b.employee)
    },
    {
      title: 'Проработано',
      dataIndex: 'isResearched',
      key: 'isResearched',
      filters: Array.from(new Set(grades?.map((item) => item.isResearched)))
        .map(isResearched => ({text: isResearched ? "Проработана" : "Не проработана", value: isResearched})),
      onFilter: (value, record) => record.isResearched === value,
      sorter: (a, b) => Number(a.isResearched) - Number(b.isResearched)
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
        style={{width: 200}}
      />
      {(!grades || grades.length === 0) &&
        <Card>
          <Typography>Оценок за эту дату нет.</Typography>
        </Card>
      }
      {
        grades && grades.length > 0 &&
        <>
          <Table dataSource={grades} columns={columns}/>
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

export default GradeScreen;