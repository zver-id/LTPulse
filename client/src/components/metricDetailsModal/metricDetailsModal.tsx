import {DatePicker, Flex, Modal, Table, Typography} from "antd";
import {useGetTicketByMetricQuery} from "../../storage/services/tickets-api.ts";
import {useGetTableSchemaQuery} from "../../storage/services/tableSchema-api.ts";
import { useState } from "react";
import dayjs from "dayjs";

interface metricDetailsModalProps {
  teamId: number;
  date: string;
  metric: string;
  isOpen: boolean;
  onClose: () => void;
}

function MetricDetailsModal({ teamId, date, metric, isOpen, onClose }: metricDetailsModalProps) {
  const [dateOfMetric, setDateOfMetric] = useState<string>(date)
  const {data} = useGetTicketByMetricQuery({teamId, date: dateOfMetric, metric}, {skip: !isOpen });
  console.log(data)
  const {data: columns} = useGetTableSchemaQuery({typeOfTable: "ticket"})
  console.log(columns)
  return (
      <Modal
        open={isOpen}
        onCancel={onClose}
        cancelText="Закрыть"
        okButtonProps={{ style: { display: 'none' } }}
        width="auto" //TODO убрать в стили
        style={{ maxWidth: '90vw' }}
      >
        <h2>{`Обращения по метрике ${metric}`}</h2>
        <Flex>
          <Typography>Дата метрики: </Typography>
          <DatePicker
            value={dateOfMetric ? dayjs(dateOfMetric) : null}
            onChange={(newDate) => setDateOfMetric(newDate ? newDate.format("YYYY-MM-DD") : "")}
            format="DD-MM-YYYY"
            style={{width: 200}}
          />
        </Flex>
        <Table dataSource={data} columns={columns} />
      </Modal>
  )
}

export default MetricDetailsModal;

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