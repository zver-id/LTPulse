import {Modal, Table} from "antd";
import {useGetTicketByMetricQuery} from "../../storage/services/tickets-api.ts";
import {useGetTableSchemaQuery} from "../../storage/services/tableSchema-api.ts";

interface metricDetailsModalProps {
  teamId: number;
  date: string;
  metric: string;
  isOpen: boolean;
  onClose: () => void;
}

function MetricDetailsModal({ teamId, date, metric, isOpen, onClose }: metricDetailsModalProps) {
  const {data} = useGetTicketByMetricQuery({teamId, date, metric}, {skip: !isOpen });
  const {data: columns} = useGetTableSchemaQuery({typeOfTable: "tickets"})
  return (
      <Modal
        open={isOpen}
        onCancel={onClose}
        cancelText="Закрыть"
        okButtonProps={{ style: { display: 'none' } }}
      >
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