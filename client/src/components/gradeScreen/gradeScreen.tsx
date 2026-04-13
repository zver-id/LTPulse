import {useState} from "react";
import dayjs, {Dayjs} from "dayjs";
import type {ColumnsType, ColumnType} from "antd/es/table";
import {Button, Card, DatePicker, Space, Table, Typography} from "antd";
import {EditOutlined} from "@ant-design/icons";
import EditTicketModal from "../editTicketModal/editTicketModal.tsx";
import {useGetGradesQuery} from "../../storage/services/grade-api.ts";


export interface IGrade {
  key: number,
  text: string,
  score: number,
  date: string,
  isResearched: boolean,
  hyperlink: string,
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

  const handleEdit = (record: IGrade) => {
    setSelectedGrade(record);
    setModalOpen(true);
  };

  const handleCloseModal = () => {
    setModalOpen(false);
    setSelectedGrade(null);
    setSaving(false);
  };

  const dataColumns: ColumnsType<IGrade> = Object.entries(grades?.schema || {})
    .filter(([key]) => key.toLowerCase() !== 'hyperlink')
    .map(([key, displayName]) => {

    const baseColumn: ColumnType<IGrade> = {
      title: displayName,
      dataIndex: key.toLowerCase(),
      key: key.toLowerCase(),
    }

    if (key.toLowerCase() === 'key') {
      baseColumn.render = (text: string, record: IGrade) => {
        return <a href={record.hyperlink} target="_blank">
          {text}
        </a>;
      }
    }

    if (grades?.data.length && grades.data.length > 1) {
      baseColumn.sorter = (a: IGrade, b: IGrade) =>{
        const aVal = a[key as keyof IGrade];
        const bVal = b[key as keyof IGrade];
        if (typeof aVal === 'string' && typeof bVal === 'string') {
          return aVal.localeCompare(bVal)
        }
        else if (typeof aVal === 'number' && typeof bVal === 'number') {
          return aVal - bVal
        }
        else {
          return Number(aVal) - Number(bVal)
        }
      }
    }
    return baseColumn;
  });

  const actionColumns: ColumnType<IGrade> =
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

  const columns: ColumnsType<IGrade> = [...dataColumns, actionColumns];

  return (
    <div>
      <DatePicker
        value={dateJs}
        onChange={(newDate) => setDateJs(newDate)}
        format="DD-MM-YYYY"
        style={{width: 200}}
      />
      {(!grades || grades.data.length === 0) &&
        <Card>
          <Typography>Оценок за эту дату нет.</Typography>
        </Card>
      }
      {
        grades && grades.data.length > 0 &&
        <>
          <Table dataSource={grades.data} columns={columns}/>
          <EditTicketModal isOpen={modalOpen}
                           grade={selectedGrade}
                           loading={saving}
                           onClose={handleCloseModal}
                           onSuccess={handleEdit}/>
        </>
      }
    </div>
  )
}

export default GradeScreen;