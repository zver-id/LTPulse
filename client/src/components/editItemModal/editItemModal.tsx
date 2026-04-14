import {useEffect} from 'react';
import {Form, Input, message, Modal} from 'antd';
import type {ITicket} from '../ticketsScreen/ticketScreen.tsx';
import TextArea from "antd/es/input/TextArea";
import {useUpdateTicketMutation} from "../../storage/services/tickets-api.ts"
import type {IGrade} from "../gradeScreen/gradeScreen.tsx";

interface EditItemModalProps<T = ITicket | IGrade> {
  isOpen: boolean;
  item: T | null;
  loading: boolean;
  onClose: () => void;
  onSuccess: (record: T) => void;
}

function EditItemModal<T extends ITicket | IGrade> ({isOpen, item, loading, onClose, onSuccess}: EditItemModalProps<T>) {

  const [updateTicket] = useUpdateTicketMutation();
  const [form] = Form.useForm();

  useEffect(() => {
    if (item && isOpen) {
      form.setFieldsValue({...item});
    } else if (!isOpen) {
      form.resetFields();
    }
  }, [item, isOpen, form]);

  const handleSubmit = async () => {
    try {
      const formValues = await form.validateFields();
      const values = {...item, ...formValues};

      await updateTicket(values).unwrap();
      message.success('Комментарий успешно обновлен');

      onSuccess(values);
      onClose();

    } catch (error) {
      message.error(`Не удалось сохранить изменения ${error}`);
    }
  };

  return (
    <Modal
      title={`Обращение ${item?.key}`}
      open={isOpen}
      onOk={handleSubmit}
      onCancel={onClose}
      confirmLoading={loading}
      okText="Сохранить"
      cancelText="Отмена"
      width={600}
      destroyOnHidden
    >
      <Form
        form={form}
        layout="vertical"
        name="editTicketForm"
      >
        <Form.Item name="name" label="Описание">
          <Input disabled/>
        </Form.Item>

        <Form.Item name="organization" label="Организация">
          <Input disabled/>
        </Form.Item>

        <Form.Item name="comment" label="Комментарий">
          <TextArea autoSize/>
        </Form.Item>

      </Form>
    </Modal>
  );
};

export default EditItemModal;