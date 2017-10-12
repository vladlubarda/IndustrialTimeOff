using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IndustrialTimeOff
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load( object sender, EventArgs e )
        {
            // TODO: This line of code loads data into the 'timeOffDatabaseDataSet.TimeOff' table. You can move, or remove it, as needed.
            //            this.timeOffTableAdapter.Fill(this.timeOffDatabaseDataSet.TimeOff);
            // TODO: This line of code loads data into the 'timeOffDatabaseDataSet.Employees' table. You can move, or remove it, as needed.
            this.employeesTableAdapter.Fill( this.timeOffDatabaseDataSet.Employees );

            //this.timeOffTableAdapter.Fill( this.timeOffDatabaseDataSet1.TimeOff );

            int employeeId = Convert.ToInt32( dataGridView1.Rows[ 0 ].Cells[ 0 ].Value );
            timeOffTableAdapter.Fill( this.timeOffDatabaseDataSet.TimeOff, employeeId );
        }

        private void SaveEmployee_Click( object sender, EventArgs e )
        {
            try
            {
                this.employeesTableAdapter.Update( this.timeOffDatabaseDataSet.Employees );

                //int employeeId = Convert.ToInt32( dataGridView1.SelectedRows.List[ 0 ].Index );
                //timeOffTableAdapter.Fill( this.timeOffDatabaseDataSet.TimeOff, employeeId );

                MessageBox.Show( "Employees table successfully updated", "Employees Update", MessageBoxButtons.OK, MessageBoxIcon.Information );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void dataGridView1_DataError( object sender, DataGridViewDataErrorEventArgs e )
        {
            if ( e.Exception != null )
            {
                MessageBox.Show( e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }

        }
/*
        private void dataGridView2_RowValidating( object sender, DataGridViewCellCancelEventArgs e )
        {
            // MessageBox.Show(e.RowIndex.ToString());
            // set employeeId value from selected employeeName field
            if ( dataGridView2.Rows[ e.RowIndex ].Cells[ 0 ].Value != null )
            {
                dataGridView2.Rows[ e.RowIndex ].Cells[ 1 ].Value = dataGridView2.Rows[ e.RowIndex ].Cells[ 0 ].Value;
                //MessageBox.Show( dataGridView2.Rows[ e.RowIndex ].Cells[ 0 ].Value.ToString() );
            }
        }

        private void dataGridView2_DataError( object sender, DataGridViewDataErrorEventArgs e )
        {
            if ( e.Exception != null )
            {
                MessageBox.Show( e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }
*/
        private void SaveTimeOff_Click( object sender, EventArgs e )
        {
            try
            {
                this.timeOffTableAdapter.Update( this.timeOffDatabaseDataSet.TimeOff );
                MessageBox.Show( "Time off successfully updated", "Time Off Update", MessageBoxButtons.OK, MessageBoxIcon.Information );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void button1_Click( object sender, EventArgs e )
        {
            try
            {
                this.timeOffTableAdapter.Update( this.timeOffDatabaseDataSet.TimeOff );
                MessageBox.Show( "Time off successfully updated", "Time Off Update", MessageBoxButtons.OK, MessageBoxIcon.Information );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void button2_Click( object sender, EventArgs e )
        {
            try
            {
                int employeeId = Int32.Parse( SelectEmployee.SelectedValue.ToString() );
                string type = SelectType.SelectedItem.ToString();
                DateTime start = dateTimePicker1.Value.Date;
                DateTime end = dateTimePicker2.Value.Date;

                // Create a new row.
                TimeOffDatabaseDataSet.TimeOffRow newRow;
                newRow = timeOffDatabaseDataSet.TimeOff.NewTimeOffRow();
                newRow.EmployeeId = employeeId;
                newRow.Type = type;
                newRow.StartDate = start;
                newRow.EndDate = end;

                timeOffDatabaseDataSet.TimeOff.Rows.Add( newRow );
                this.timeOffTableAdapter.Update( this.timeOffDatabaseDataSet.TimeOff );
                MessageBox.Show( "Time off table successfully updated", "TimeOff Update", MessageBoxButtons.OK, MessageBoxIcon.Information );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void comboBox1_SelectedIndexChanged( object sender, EventArgs e )
        {
            updateTimeUsedAvailable();
            //int employeeId = Int32.Parse( comboBox1.SelectedValue.ToString() );

            //if ( employeeId < 0 )
            //    return;

            ////BindingSource bindingSource1 = new BindingSource();
            ////bindingSource1.DataSource = GetData( String.Format( "Select Type, StartDate, EndDate From TimeOff where EmployeeId = {0}", employeeId ) );

            ////dataGridView4.DataSource = bindingSource1;

            //TimeOffCalculator timeOffCalculator = new TimeOffCalculator( employeeId );
            //AvailableSickDays.Text = timeOffCalculator.CalculateSickDays();
            //AvailableVacationDays.Text = timeOffCalculator.CalculateVacationDays();
            //AvailableFloatingDays.Text = timeOffCalculator.CalculateFloatingDays();

//            timeOffTableAdapter.Fill( this.timeOffDatabaseDataSet.TimeOff, employeeId );
        }

        private void updateTimeUsedAvailable()
        {
            if ( comboBox1.SelectedValue != null )
            {
                int employeeId = Int32.Parse( comboBox1.SelectedValue.ToString() );

                if ( employeeId < 0 )
                    return;

                TimeOffCalculator timeOffCalculator = new TimeOffCalculator( employeeId );
                AvailableSickDays.Text = timeOffCalculator.CalculateSickDays();
                AvailableVacationDays.Text = timeOffCalculator.CalculateVacationDays();
                AvailableFloatingDays.Text = timeOffCalculator.CalculateFloatingDays();
            }
        }


        private static DataTable GetData( string sqlCommand )
        {

            //SqlConnection con = new SqlConnection( "IndustrialTimeOff.Properties.Settings.TimeOffDatabaseConnectionString" );
            SqlConnection con = new SqlConnection();
            con.ConnectionString = global::IndustrialTimeOff.Properties.Settings.Default.TimeOffDatabaseConnectionString;

            // IndustrialTimeOff.Properties.Settings.TimeOffDatabaseConnectionString;

            SqlCommand command = new SqlCommand( sqlCommand, con );
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = command;

            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            adapter.Fill( table );

            return table;
        }
        /*
                private void Reports_Enter( object sender, EventArgs e )
                {
                    //BindingSource bindingSource1 = new BindingSource();
                    //bindingSource1.DataSource = GetData( "Select Name, Id From Employees" );


                    //// this.comboBox1.DataSource = employeesBindingSource;
                    //this.comboBox1.DataSource = bindingSource1;
                    //this.comboBox1.DisplayMember = "Name";
                    //this.comboBox1.ValueMember = "Id";

                    ////this.comboBox1.SelectedItem = -1;
                    ////this.comboBox1.SelectedText = "";
                    ////this.comboBox1.Text = "";
                }

                private void Adjustments_Enter( object sender, EventArgs e )
                {
                    BindingSource bindingSource1 = new BindingSource();
                    bindingSource1.DataSource = GetData( "Select Name, Id From Employees" );


                    // this.comboBox1.DataSource = employeesBindingSource;
                    this.comboBox2.DataSource = bindingSource1;
                    this.comboBox2.DisplayMember = "Name";
                    this.comboBox2.ValueMember = "Id";
                }
        */
        private void comboBox2_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( comboBox2.SelectedValue != null )
            { 
                int employeeId = Int32.Parse( comboBox2.SelectedValue.ToString() );

                if ( employeeId < 0 )
                    return;

                timeOffTableAdapter.Fill( this.timeOffDatabaseDataSet.TimeOff, employeeId );
            }
        }

        private void SaveChanges_Click( object sender, EventArgs e )
        {
            try
            {
                timeOffTableAdapter.Update( this.timeOffDatabaseDataSet.TimeOff );
                MessageBox.Show( "Time Off successfully updated", "Time Off Update", MessageBoxButtons.OK, MessageBoxIcon.Information );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void dateTimePicker1_ValueChanged( object sender, EventArgs e )
        {
            this.dateTimePicker2.Value = this.dateTimePicker1.Value;
        }

        private void Reports_Enter( object sender, EventArgs e )
        {
            updateTimeUsedAvailable();
        }

        private void dataGridView3_DataError( object sender, DataGridViewDataErrorEventArgs e )
        {

        }

        private void dataGridView4_DataError( object sender, DataGridViewDataErrorEventArgs e )
        {

        }
    }

}
